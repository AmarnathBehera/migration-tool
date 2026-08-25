using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Options;
using CMS.ContentEngine;
using CMS.Websites;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class PageAttachmentServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Mock<IOptions<PageAttachmentOptions>> _optionsMock = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _optionsMock = new Mock<IOptions<PageAttachmentOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(new PageAttachmentOptions());

        }

        private PageAttachmentService CreateSut() => new(
            _contentServiceMock.Object,
            _httpClientFactoryMock.Object,
            _httpContextAccessorMock.Object,
            _optionsMock.Object);

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static BDO.ContentAttachment CreateAttachment(Guid guid, ContentItemAsset? asset = null)
    {
        var attachment = new BDO.ContentAttachment();
        typeof(BDO.ContentAttachment)
            .GetProperty("SystemFields")!
            .SetValue(attachment, new ContentItemFields { ContentItemGUID = guid });
        attachment.ContentAttachmentAsset = asset!;
        return attachment;
    }

    private static ContentItemAsset CreateAsset(string url, string name, string extension)
    {
        var asset = new ContentItemAsset();

        var metadata = (ContentItemAssetMetadata)RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAssetMetadata));
        SetProperty(metadata, "Name", name);
        SetProperty(metadata, "Extension", extension);
        SetProperty(metadata, "Size", 0L);
        SetProperty(metadata, "Width", (int?)null);
        SetProperty(metadata, "Height", (int?)null);
        SetProperty(metadata, "LastModified", (DateTime?)null);

        typeof(ContentItemAsset).GetProperty("Metadata")!.SetValue(asset, metadata);
        typeof(ContentItemAsset).GetProperty("Url")!.SetValue(asset, url);

        return asset;
    }

    private static void SetProperty(object target, string name, object? value)
    {
        var type = target.GetType();
        PropertyInfo? prop = null;
        while (type != null && prop == null)
        {
            prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            type = type.BaseType;
        }
        prop!.SetValue(target, value);
    }

    /// <summary>A minimal IWebPageFieldsSource implementation that exposes a ContentAttachment list.</summary>
    private class FakePage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new();
        public IEnumerable<BDO.ContentAttachment> Attachments { get; set; } = [];
    }

    private void SetupContentService(Guid pageGuid, IEnumerable<IWebPageFieldsSource>? pages) =>
        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDForAPIAsync(
                It.Is<List<Guid>>(l => l.Contains(pageGuid)),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(pages!);

    private void SetupHttpResponse(HttpStatusCode statusCode, byte[]? body = null, string? mediaType = null)
    {
        var content = body is null
            ? new ByteArrayContent([])
            : new ByteArrayContent(body);

        if (mediaType != null)
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode) { Content = content });
    }

    private void SetupLiveSiteBaseUrl(string url) =>
        _optionsMock.Setup(o => o.Value).Returns(new PageAttachmentOptions { LiveSiteBaseUrl = url });

    // ── GetPageAttachmentsAsync ────────────────────────────────────────────────

    [Test]
    public async Task GetPageAttachmentsAsync_WhenContentServiceReturnsNull_ReturnsEmpty()
    {
        var pageGuid = Guid.NewGuid();
        SetupContentService(pageGuid, null);

        var result = await CreateSut().GetPageAttachmentsAsync(pageGuid, "testchannel", "en-gb");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPageAttachmentsAsync_WhenPageHasNoAttachmentProperties_ReturnsEmpty()
    {
        var pageGuid = Guid.NewGuid();
        // FakePage with empty attachments list
        var page = new FakePage { Attachments = [] };
        SetupContentService(pageGuid, [page]);

        var result = await CreateSut().GetPageAttachmentsAsync(pageGuid, "testchannel", "en-gb");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetPageAttachmentsAsync_WhenPageHasAttachments_ReturnsAllAttachments()
    {
        var pageGuid = Guid.NewGuid();
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var page = new FakePage
        {
            Attachments =
            [
                CreateAttachment(guid1),
                CreateAttachment(guid2)
            ]
        };
        SetupContentService(pageGuid, [page]);

        var result = (await CreateSut().GetPageAttachmentsAsync(pageGuid, "testchannel", "en-gb")).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(a => a.SystemFields.ContentItemGUID),
            Is.EquivalentTo(new[] { guid1, guid2 }));
    }

    [Test]
    public async Task GetPageAttachmentsAsync_AggregatesAttachmentsAcrossMultiplePages()
    {
        var pageGuid = Guid.NewGuid();
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var page1 = new FakePage { Attachments = [CreateAttachment(guid1)] };
        var page2 = new FakePage { Attachments = [CreateAttachment(guid2)] };
        SetupContentService(pageGuid, [page1, page2]);

        var result = (await CreateSut().GetPageAttachmentsAsync(pageGuid, "testchannel", "en-gb")).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
    }

    // ── GetPageAttachmentBinaryAsync ───────────────────────────────────────────

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenAttachmentNotFound_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var differentGuid = Guid.NewGuid();
        var page = new FakePage { Attachments = [CreateAttachment(differentGuid)] };
        SetupContentService(pageGuid, [page]);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenAssetIsNull_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset: null)] };
        SetupContentService(pageGuid, [page]);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenAssetUrlIsNull_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        // Asset with no URL (Url property remains null)
        var asset = new ContentItemAsset();
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenNoLiveSiteBaseUrlAndNoHttpContext_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

        // No LiveSiteBaseUrl and no HttpContext
        _optionsMock.Setup(o => o.Value).Returns(new PageAttachmentOptions { LiveSiteBaseUrl = null });
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenHttpRequestFails_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupLiveSiteBaseUrl("https://example.com");
        SetupHttpResponse(HttpStatusCode.NotFound);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenSuccessful_ReturnsBinaryData()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 1, 2, 3, 4 };
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupLiveSiteBaseUrl("https://example.com");
        SetupHttpResponse(HttpStatusCode.OK, expectedBytes, "application/pdf");

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Value.Data, Is.EqualTo(expectedBytes));
            Assert.That(result.Value.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.Value.FileName, Is.EqualTo("file.pdf"));
        });
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenContentTypeHeaderMissing_FallsBackToMimeTypeFromExtension()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 5, 6, 7 };
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupLiveSiteBaseUrl("https://example.com");
        // No media type on the response — service must fall back to extension-based MIME
        SetupHttpResponse(HttpStatusCode.OK, expectedBytes, mediaType: null);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.ContentType, Is.EqualTo("application/pdf"));
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenExtensionUnknown_FallsBackToOctetStream()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("/media/file.xyz", "file.xyz", ".xyz");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupLiveSiteBaseUrl("https://example.com");
        SetupHttpResponse(HttpStatusCode.OK, [1, 2], mediaType: null);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.ContentType, Is.EqualTo(MediaTypeNames.Application.Octet));
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_UsesHttpContextWhenNoLiveSiteBaseUrl()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("~/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

        // No LiveSiteBaseUrl — should fall back to HttpContext request
        _optionsMock.Setup(o => o.Value).Returns(new PageAttachmentOptions { LiveSiteBaseUrl = null });

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Scheme).Returns("https");
        mockRequest.Setup(r => r.Host).Returns(new HostString("mysite.com"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        SetupHttpResponse(HttpStatusCode.OK, [1, 2, 3], "application/pdf");

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Data, Is.EqualTo(new byte[] { 1, 2, 3 }));
    }
}
