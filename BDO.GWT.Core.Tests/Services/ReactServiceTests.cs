using BDO.GWT.Common.Exceptions;
using BDO.GWT.Common.Options;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class ReactServiceTests
{

    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IOptions<ReactOptions>> _reactOptionsMock;
    private Mock<IReactCachingService> _reactCachingServiceMock;
    private Mock<ILogger<ReactService>> _loggerMock;

    private const int PageId = 1;
    private const string ComponentType = "TestComponent";
    private const string PersistentUniqueIdentifier = "test-uid-001";

    private static readonly Uri ServerUrl = new("http://localhost:3000");

    private static readonly ReactResponseBodyDto DefaultResponseBody = new()
    {
        ContainerId = "container-abc",
        HtmlContent = "<div id=\"container-abc\">rendered</div>",
    };

    [SetUp]
    public void SetUp()
    {
        // --- HTTP plumbing ---
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = ServerUrl,
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(HttpClientName.React))
            .Returns(httpClient);

        // --- ReactOptions ---
        _reactOptionsMock = new Mock<IOptions<ReactOptions>>();
        _reactOptionsMock
            .Setup(o => o.Value)
            .Returns(new ReactOptions { ServerUrl = ServerUrl });

        // --- Caching service: by default pass-through to the factory ---
        _reactCachingServiceMock = new Mock<IReactCachingService>();
        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<ReactResponseBodyDto>>, string, CancellationToken>(
                (factory, key, ct) => factory(ct));

        // --- Logger ---
        _loggerMock = new Mock<ILogger<ReactService>>();
    }

    // Convenience factory so each test starts with a fresh service instance.
    private ReactService CreateSut() =>
        new(_httpClientFactoryMock.Object,
            _reactOptionsMock.Object,
            _reactCachingServiceMock.Object,
            _loggerMock.Object);

    /// <summary>
    /// Sets up the mock HTTP handler to return the given response for any POST.
    /// </summary>
    private void SetupHttpResponse(HttpStatusCode statusCode, object? body = null)
    {
        var json = body is null ? string.Empty : JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode) { Content = content });
    }

    // -----------------------------------------------------------------------
    // TC-01  Happy path: RenderComponentAsync returns rendered HTML
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenServerReturnsHtml_ReturnsExpectedHtmlContent()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, DefaultResponseBody);
        var sut = CreateSut();

        // Act
        var result = await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { Title = "Hello" }, CancellationToken.None);

        // Assert
        Assert.That(result.ToString(), Does.Contain(DefaultResponseBody.HtmlContent));
    }

    // -----------------------------------------------------------------------
    // TC-02  Cache hit: LoadAsync is called with the expected composite key
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_AlwaysCallsCachingService_WithCompositeKey()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, DefaultResponseBody);
        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { Value = 42 }, CancellationToken.None);

        // Assert – the cache key must embed pageId and persistentUniqueIdentifier
        _reactCachingServiceMock.Verify(
            c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.Is<string>(k => k.StartsWith($"{PageId}|{PersistentUniqueIdentifier}|")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // TC-03  Cache is used: HTTP is NOT called when cache returns without invoking factory
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenCacheHit_DoesNotCallHttpClient()
    {
        // Arrange – caching service returns directly without calling the factory
        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultResponseBody);

        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { Cached = true }, CancellationToken.None);

        // Assert – HTTP handler should never have been touched
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    // -----------------------------------------------------------------------
    // TC-04  configureOptions overload: options are applied before the cache key is built
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WithConfigureOptions_InvokesOptionsDelegate()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, DefaultResponseBody);
        var sut = CreateSut();
        var delegateInvoked = false;

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { X = 1 },
            opts => { delegateInvoked = true; },
            CancellationToken.None);

        // Assert
        Assert.That(delegateInvoked, Is.True);
    }

    // -----------------------------------------------------------------------
    // TC-05  Empty body: logs an error and returns empty HtmlContent (no throw)
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenServerReturnsEmptyBody_LogsErrorAndReturnsEmpty()
    {
        // Arrange – return 200 with empty string body
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });

        var sut = CreateSut();

        // Act
        var result = await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { A = "b" }, CancellationToken.None);

        // Assert – should not throw; HTML is empty; error was logged
        Assert.That(result.ToString(), Is.EqualTo(string.Empty).Or.Empty);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(ComponentType)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // TC-06  Non-success HTTP status: throws (via HttpResponseUtility)
    // -----------------------------------------------------------------------
    [Test]
    public void RenderComponentAsync_WhenServerReturnsNonSuccess_Throws()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError);
        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsAsync<UnsuccessfulResponseException>(async () =>
            await sut.RenderComponentAsync(
                PageId, ComponentType, PersistentUniqueIdentifier,
                new { Fail = true }, CancellationToken.None));
    }

    // -----------------------------------------------------------------------
    // TC-07  RenderCombinedScript: empty when no components rendered
    // -----------------------------------------------------------------------
    [Test]
    public void RenderCombinedScript_WithNoRenderedComponents_ReturnsScriptTagsOnly()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.RenderCombinedScript();
        var html = RenderHtmlContent(result);

        // Assert – should contain the script wrapper but no hydrate calls
        Assert.That(html, Does.Contain("<script>"));
        Assert.That(html, Does.Contain("</script>"));
        Assert.That(html, Does.Not.Contain("widgetRenderer.hydrate"));
    }

    // -----------------------------------------------------------------------
    // TC-08  RenderCombinedScript: emits one hydrate call per rendered component
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderCombinedScript_AfterRenderingComponents_EmitsHydratePerComponent()
    {
        // Arrange
        var body1 = new ReactResponseBodyDto { ContainerId = "c1", HtmlContent = "<div id='c1'/>" };
        var body2 = new ReactResponseBodyDto { ContainerId = "c2", HtmlContent = "<div id='c2'/>" };

        var callCount = 0;
        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? body1 : body2);

        var sut = CreateSut();

        await sut.RenderComponentAsync(PageId, "CompA", "uid-1", new { N = 1 }, CancellationToken.None);
        await sut.RenderComponentAsync(PageId, "CompB", "uid-2", new { N = 2 }, CancellationToken.None);

        // Act
        var html = RenderHtmlContent(sut.RenderCombinedScript());

        // Assert
        Assert.That(html, Does.Contain("widgetRenderer.hydrate('CompA', 'c1'"));
        Assert.That(html, Does.Contain("widgetRenderer.hydrate('CompB', 'c2'"));
    }

    // -----------------------------------------------------------------------
    // TC-08b  Two instances served one cached render (identical props, e.g. a
    //         copy/pasted widget) must not share a container id, otherwise every
    //         hydrate() call resolves to the first matching DOM element.
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenCachedRenderIsReusedOnSamePage_AssignsDistinctContainerIds()
    {
        // Arrange - the cache hands back the very same instance for both calls
        var cached = new ReactResponseBodyDto
        {
            ContainerId = "container-abc",
            HtmlContent = "<div id=\"container-abc\">rendered</div>",
        };

        SetupCacheToAlwaysReturn(cached);

        var sut = CreateSut();

        // Act - same component, identical props, exactly as a copy/pasted widget produces
        var first = RenderHtmlContent(await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None));
        var second = RenderHtmlContent(await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None));

        var script = RenderHtmlContent(sut.RenderCombinedScript());
        var secondId = ExtractContainerId(second);

        Assert.Multiple(() =>
        {
            // First instance keeps the renderer's id, the second is re-issued
            Assert.That(first, Does.Contain("id=\"container-abc\""));
            Assert.That(second, Does.Not.Contain("id=\"container-abc\""));
            Assert.That(secondId, Does.Match("^react_[0-9a-f]{32}$"));

            // Inner content survives the HtmlAgilityPack round-trip
            Assert.That(second, Does.Contain("rendered"));

            // Hydration targets two different containers
            Assert.That(script, Does.Contain($"widgetRenderer.hydrate('{ComponentType}', 'container-abc'"));
            Assert.That(script, Does.Contain($"widgetRenderer.hydrate('{ComponentType}', '{secondId}'"));
        });
    }

    // -----------------------------------------------------------------------
    // TC-08c  The cached DTO is shared across requests and must never be mutated
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenRewritingContainerId_DoesNotMutateCachedResponse()
    {
        // Arrange
        var cached = new ReactResponseBodyDto
        {
            ContainerId = "container-abc",
            HtmlContent = "<div id=\"container-abc\">rendered</div>",
        };

        SetupCacheToAlwaysReturn(cached);

        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None);
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None);

        // Assert - a mutated cache entry would poison every later request
        Assert.Multiple(() =>
        {
            Assert.That(cached.ContainerId, Is.EqualTo("container-abc"));
            Assert.That(cached.HtmlContent, Is.EqualTo("<div id=\"container-abc\">rendered</div>"));
        });
    }

    // -----------------------------------------------------------------------
    // TC-08d  A third identical instance also gets its own container id
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenCachedRenderIsReusedThreeTimes_AllContainerIdsAreUnique()
    {
        // Arrange
        SetupCacheToAlwaysReturn(new ReactResponseBodyDto
        {
            ContainerId = "c1",
            HtmlContent = "<div id=\"c1\">rendered</div>",
        });

        var sut = CreateSut();
        var ids = new List<string>();

        // Act
        for (var i = 0; i < 3; i++)
        {
            var html = RenderHtmlContent(await sut.RenderComponentAsync(
                PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None));

            ids.Add(ExtractContainerId(html));
        }

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ids[0], Is.EqualTo("c1"));
            Assert.That(ids, Is.Unique);
        });
    }

    // -----------------------------------------------------------------------
    // TC-08e  Distinct props are separate cache entries with their own container
    //         ids already, so nothing must be rewritten
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenContainerIdsAlreadyDiffer_LeavesHtmlUntouched()
    {
        // Arrange
        var bodies = new Queue<ReactResponseBodyDto>(
        [
            new ReactResponseBodyDto { ContainerId = "c1", HtmlContent = "<div id=\"c1\">a</div>" },
            new ReactResponseBodyDto { ContainerId = "c2", HtmlContent = "<div id=\"c2\">b</div>" },
        ]);

        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bodies.Dequeue);

        var sut = CreateSut();

        // Act
        var first = RenderHtmlContent(await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None));
        var second = RenderHtmlContent(await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 2 }, CancellationToken.None));

        // Assert - returned verbatim, no HtmlAgilityPack round-trip
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("<div id=\"c1\">a</div>"));
            Assert.That(second, Is.EqualTo("<div id=\"c2\">b</div>"));
        });
    }

    // -----------------------------------------------------------------------
    // TC-08f  An empty container id (the empty-response fallback) is left alone
    //         rather than being treated as a duplicate
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenContainerIdIsEmpty_DoesNotRewrite()
    {
        // Arrange
        SetupCacheToAlwaysReturn(new ReactResponseBodyDto
        {
            ContainerId = string.Empty,
            HtmlContent = string.Empty,
        });

        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None);
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None);

        var script = RenderHtmlContent(sut.RenderCombinedScript());

        // Assert - both hydrate calls keep the empty id, no GUID is issued
        Assert.That(script, Does.Not.Contain("react_"));
    }

    // -----------------------------------------------------------------------
    // TC-08g  A duplicate whose HTML does not actually contain the container
    //         element is surfaced as UnexpectedBodyException
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_WhenDuplicateHtmlHasNoContainerElement_ThrowsUnexpectedBodyException()
    {
        // Arrange - ContainerId does not match any id in the markup
        SetupCacheToAlwaysReturn(new ReactResponseBodyDto
        {
            ContainerId = "container-abc",
            HtmlContent = "<div id=\"something-else\">rendered</div>",
        });

        var sut = CreateSut();

        // First render succeeds - the duplicate branch is not entered yet
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None);

        // Act & Assert
        Assert.ThrowsAsync<UnexpectedBodyException>(async () =>
            await sut.RenderComponentAsync(
                PageId, ComponentType, PersistentUniqueIdentifier, new { N = 1 }, CancellationToken.None));
    }

    /// <summary>
    /// Makes the caching service return the same instance for every call, which is what
    /// happens when several widgets on a page hash to the same cache key.
    /// </summary>
    private void SetupCacheToAlwaysReturn(ReactResponseBodyDto body) =>
        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(body);

    private static string ExtractContainerId(string html) =>
        System.Text.RegularExpressions.Regex.Match(html, "id=\"([^\"]+)\"").Groups[1].Value;

    // -----------------------------------------------------------------------
    // TC-09  RenderCombinedScript: props are serialized with camelCase policy
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderCombinedScript_PropsAreSerializedWithCamelCase()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, DefaultResponseBody);
        var sut = CreateSut();

        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { MyProp = "value" }, CancellationToken.None);

        // Act
        var html = RenderHtmlContent(sut.RenderCombinedScript());

        // Assert – camelCase key, not PascalCase
        Assert.That(html, Does.Contain("\"myProp\""));
        Assert.That(html, Does.Not.Contain("\"MyProp\""));
    }

    // -----------------------------------------------------------------------
    // TC-10  HTTP POST targets the correct URL path  /api/render/{componentType}
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_PostsToCorrectUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(DefaultResponseBody),
            });

        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(
            PageId, ComponentType, PersistentUniqueIdentifier,
            new { }, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest!.RequestUri!.AbsolutePath,
                Is.EqualTo($"/api/render/{ComponentType}"));
            Assert.That(capturedRequest.Method, Is.EqualTo(HttpMethod.Post));

        });
        
    }

    // -----------------------------------------------------------------------
    // TC-11  Null body from deserializer: throws UnexpectedBodyException
    // -----------------------------------------------------------------------
    [Test]
    public void RenderComponentAsync_WhenDeserializationReturnsNull_ThrowsUnexpectedBodyException()
    {
        // Arrange – return whitespace-containing JSON that cannot deserialize as ReactResponseBodyDto
        // Simplest approach: return the literal JSON "null"
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json"),
            });

        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsAsync<UnexpectedBodyException>(async () =>
            await sut.RenderComponentAsync(
                PageId, ComponentType, PersistentUniqueIdentifier,
                new { }, CancellationToken.None));
    }

    // -----------------------------------------------------------------------
    // TC-12  Different prop shapes produce different cache keys (hash differs)
    // -----------------------------------------------------------------------
    [Test]
    public async Task RenderComponentAsync_DifferentProps_ProduceDifferentCacheKeys()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, DefaultResponseBody);
        var capturedKeys = new List<string>();

        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<Func<CancellationToken, Task<ReactResponseBodyDto>>, string, CancellationToken>(
                (_, key, _) => capturedKeys.Add(key))
            .ReturnsAsync(DefaultResponseBody);

        var sut = CreateSut();

        // Act
        await sut.RenderComponentAsync(PageId, ComponentType, PersistentUniqueIdentifier,
            new { Value = 1 }, CancellationToken.None);

        await sut.RenderComponentAsync(PageId, ComponentType, PersistentUniqueIdentifier,
            new { Value = 2 }, CancellationToken.None);

        // Assert
        Assert.That(capturedKeys, Has.Count.EqualTo(2));
        Assert.That(capturedKeys[0], Is.Not.EqualTo(capturedKeys[1]));
    }

    // -----------------------------------------------------------------------
    // TC-13  Cancellation: operation is cancelled when token is already cancelled
    // -----------------------------------------------------------------------
    [Test]
    public void RenderComponentAsync_WhenCancellationRequested_ThrowsTaskCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Caching service must forward the cancellation to the HTTP call
        _reactCachingServiceMock
            .Setup(c => c.LoadAsync(
                It.IsAny<Func<CancellationToken, Task<ReactResponseBodyDto>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<ReactResponseBodyDto>>, string, CancellationToken>(
                (factory, key, ct) => factory(ct));   // pass-through so HTTP is hit

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((_, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await sut.RenderComponentAsync(
                PageId, ComponentType, PersistentUniqueIdentifier,
                new { }, cts.Token));
    }

    private static string RenderHtmlContent(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}