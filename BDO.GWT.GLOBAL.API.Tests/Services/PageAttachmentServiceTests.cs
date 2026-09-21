using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.Core;
using CMS.Websites;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class PageAttachmentServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Mock<IWebHostEnvironment> _hostEnvironmentMock = null!;
    private Mock<IEventLogService> _eventLogServiceMock = null!;
    private Mock<IChannelWebsiteDomainService> _channelWebsiteDomainServiceMock = null!;
    private IAssetUrlResolver _assetUrlResolver = null!;

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
        _hostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _eventLogServiceMock = new Mock<IEventLogService>();
        _channelWebsiteDomainServiceMock = new Mock<IChannelWebsiteDomainService>();
        _assetUrlResolver = new AssetUrlResolver(_httpContextAccessorMock.Object, _hostEnvironmentMock.Object);
        SetupIsDevelopment(true);
    }

    private PageAttachmentService CreateSut() => new(
        _contentServiceMock.Object,
        _httpClientFactoryMock.Object,
        new AssetUrlResolver(_httpContextAccessorMock.Object, _hostEnvironmentMock.Object),
        _eventLogServiceMock.Object,
        _channelWebsiteDomainServiceMock.Object);

    private void SetupIsDevelopment(bool isDevelopment) =>
        _hostEnvironmentMock
            .Setup(x => x.EnvironmentName)
            .Returns(isDevelopment ? Environments.Development : Environments.Production);

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

    private void SetupHttpContext(string scheme = "https", string host = "localhost")
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Scheme).Returns(scheme);
        mockRequest.Setup(r => r.Host).Returns(new HostString(host));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
    }

    private void SetupChannelWebsiteDomain(string? domain) =>
        _channelWebsiteDomainServiceMock
            .Setup(x => x.GetWebsiteDomainAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

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
    public async Task GetPageAttachmentBinaryAsync_WhenAssetUrlIsAbsolute_UsesAbsoluteUrl()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 1, 2, 3 };
        var absoluteUrl = "https://media.example.com/media/file.pdf";
        var asset = CreateAsset(absoluteUrl, "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

        var content = new ByteArrayContent(expectedBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get
                    && request.RequestUri == new Uri(absoluteUrl)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Data, Is.EqualTo(expectedBytes));
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenHttpRequestFails_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupHttpContext();
        SetupHttpResponse(HttpStatusCode.NotFound);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenAssetUrlIsVirtualContextWrapped_UsesInnerAssetPath()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 1, 2, 3 };
        var asset = CreateAsset(
            "/cmsctx/pm/token/lang/en-gb/wpid/72/cid/1/cdid/82/ts/1/readonly/0/h/hash/-/~/getContentAsset/5ed65831-9e46-4827-8011-d91f5c2e130e/50c2bc4c-a8ff-46ba-95c2-0e74752d147f/file.pdf?language=en-gb",
            "file.pdf",
            ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupIsDevelopment(false);
        SetupHttpContext("https", "mysite.com");

        var content = new ByteArrayContent(expectedBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get
                    && request.RequestUri == new Uri("https://mysite.com/getContentAsset/5ed65831-9e46-4827-8011-d91f5c2e130e/50c2bc4c-a8ff-46ba-95c2-0e74752d147f/file.pdf?language=en-gb")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });

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
    public async Task GetPageAttachmentBinaryAsync_WhenChannelDomainIsAvailable_UsesChannelDomain()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 1, 2, 3, 4 };
        var asset = CreateAsset("~/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupChannelWebsiteDomain("gwt-xbyk.dev.bdo.global");

        var content = new ByteArrayContent(expectedBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get
                    && request.RequestUri == new Uri("https://gwt-xbyk.dev.bdo.global/media/file.pdf")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });

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
    public async Task GetPageAttachmentBinaryAsync_WhenSuccessful_ReturnsBinaryData()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 1, 2, 3, 4 };
        var asset = CreateAsset("/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupChannelWebsiteDomain(null);
        SetupHttpContext();
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
    public async Task GetPageAttachmentBinaryAsync_WhenMetadataIsMissing_UsesFallbackValues()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expectedBytes = new byte[] { 9, 8, 7 };
        var asset = new ContentItemAsset();
        typeof(ContentItemAsset).GetProperty("Url")!.SetValue(asset, "/media/file.pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);
        SetupHttpContext();
        SetupHttpResponse(HttpStatusCode.OK, expectedBytes, "application/pdf");

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Value.Data, Is.EqualTo(expectedBytes));
            Assert.That(result.Value.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.Value.FileName, Is.EqualTo("attachment"));
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
        SetupHttpContext();
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
        SetupHttpContext();
        SetupHttpResponse(HttpStatusCode.OK, [1, 2], mediaType: null);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.ContentType, Is.EqualTo(MediaTypeNames.Application.Octet));
    }

    [Test]
    public void BuildAssetUri_WhenProductionAndHttpContextMissing_ReturnsNull()
    {
        SetupIsDevelopment(false);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = (Uri?)typeof(PageAttachmentService)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(m => m.Name == "BuildAssetUri" && m.GetParameters().Length == 1)
            .Invoke(CreateSut(), ["~/media/file.pdf"]);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void BuildAssetUri_WhenDevelopmentAndRelativePath_UsesDevelopmentBaseUrl()
    {
        SetupIsDevelopment(true);

        var result = (Uri?)typeof(PageAttachmentService)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(m => m.Name == "BuildAssetUri" && m.GetParameters().Length == 1)
            .Invoke(CreateSut(), ["~/media/file.pdf?language=en-gb"]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AbsoluteUri, Is.EqualTo("http://localhost:12393/media/file.pdf?language=en-gb"));
    }

    [Test]
    public void BuildAssetUri_WhenVirtualContextWrappedPath_UsesInnerAssetPath()
    {
        SetupIsDevelopment(false);
        SetupHttpContext("https", "mysite.com");

        var result = (Uri?)typeof(PageAttachmentService)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(m => m.Name == "BuildAssetUri" && m.GetParameters().Length == 1)
            .Invoke(CreateSut(), ["/cmsctx/pm/token/lang/en-gb/wpid/72/cid/1/cdid/82/ts/1/readonly/0/h/hash/-/~/getContentAsset/5ed65831-9e46-4827-8011-d91f5c2e130e/50c2bc4c-a8ff-46ba-95c2-0e74752d147f/file.pdf?language=en-gb"]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AbsoluteUri, Is.EqualTo("https://mysite.com/getContentAsset/5ed65831-9e46-4827-8011-d91f5c2e130e/50c2bc4c-a8ff-46ba-95c2-0e74752d147f/file.pdf?language=en-gb"));
    }

    [Test]
    public async Task GetPageAttachmentBinaryAsync_UsesHttpContextWhenNoLiveSiteBaseUrl()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("~/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

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

    [Test]
    public async Task GetPageAttachmentBinaryAsync_WhenProductionHostIsMissing_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var asset = CreateAsset("~/media/file.pdf", "file.pdf", ".pdf");
        var page = new FakePage { Attachments = [CreateAttachment(attachmentGuid, asset)] };
        SetupContentService(pageGuid, [page]);

        SetupIsDevelopment(false);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Scheme).Returns("https");
        mockRequest.Setup(r => r.Host).Returns(default(HostString));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        var result = await CreateSut().GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch", "en-gb");

        Assert.That(result, Is.Null);
    }
}
