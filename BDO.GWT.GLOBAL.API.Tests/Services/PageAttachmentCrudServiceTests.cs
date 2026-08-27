using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.Membership;
using CMS.Websites;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class PageAttachmentCrudServiceTests
{
    private Mock<IPageAttachmentService> _pageAttachmentServiceMock = null!;
    private Mock<IPageChannelResolver> _pageChannelResolverMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock = null!;
    private Mock<IContentItemManagerFactory> _contentItemManagerFactoryMock = null!;
    private Mock<IUserInfoProvider> _userInfoProviderMock = null!;
    private PageAttachmentCrudService _service = null!;

    [SetUp]
    public void Setup()
    {
        _pageAttachmentServiceMock = new Mock<IPageAttachmentService>();
        _pageChannelResolverMock = new Mock<IPageChannelResolver>();
        _contentServiceMock = new Mock<IContentService>();
        _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>();
        _contentItemManagerFactoryMock = new Mock<IContentItemManagerFactory>();
        _userInfoProviderMock = new Mock<IUserInfoProvider>();

        _service = new PageAttachmentCrudService(
            _pageAttachmentServiceMock.Object,
            _pageChannelResolverMock.Object,
            _contentServiceMock.Object,
            _webPageManagerFactoryMock.Object,
            _contentItemManagerFactoryMock.Object,
            _userInfoProviderMock.Object);
    }

    private static BDO.ContentAttachment CreateAttachment(Guid guid, ContentItemAsset? asset = null)
    {
        var attachment = new BDO.ContentAttachment();
        typeof(BDO.ContentAttachment)
            .GetProperty("SystemFields")!
            .SetValue(attachment, new ContentItemFields { ContentItemGUID = guid });
        attachment.ContentAttachmentAsset = asset;
        return attachment;
    }

    private static ContentItemAsset CreateAsset(
        string name,
        string extension,
        long size,
        int? width = null,
        int? height = null,
        DateTime? lastModified = null)
    {
        var asset = new ContentItemAsset();
        var metadata = (ContentItemAssetMetadata)RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAssetMetadata));

        SetMetadataProperty(metadata, "Name", name);
        SetMetadataProperty(metadata, "Extension", extension);
        SetMetadataProperty(metadata, "Size", size);
        SetMetadataProperty(metadata, "Width", width);
        SetMetadataProperty(metadata, "Height", height);
        SetMetadataProperty(metadata, "LastModified", lastModified);

        typeof(ContentItemAsset)
            .GetProperty("Metadata")!
            .SetValue(asset, metadata);

        return asset;
    }

    private static void SetMetadataProperty(object target, string propertyName, object? value)
    {
        var type = target.GetType();
        PropertyInfo? property = null;

        while (type != null && property == null)
        {
            property = type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            type = type.BaseType;
        }

        property!.SetValue(target, value);
    }

    private void SetupChannel(int channelId, string channelName) =>
        _pageChannelResolverMock
            .Setup(x => x.GetChannelNameByPageGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(channelName);

    private void SetupAttachments(
        Guid pageGuid,
        string channelName,
        string culture,
        IEnumerable<BDO.ContentAttachment> attachments) =>
        _pageAttachmentServiceMock
            .Setup(x => x.GetPageAttachmentsAsync(pageGuid, channelName, culture, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachments);

    // ── GetAttachmentAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task GetAttachmentAsync_WhenAttachmentExists_ReturnsMappedOutput()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();
        var lastModified = new DateTime(2024, 6, 1);
        var asset = CreateAsset("report.pdf", ".pdf", 2048, lastModified: lastModified);
        var attachment = CreateAttachment(attachGuid, asset);

        SetupChannel(1, "testchannel");
        SetupAttachments(pageGuid, "testchannel", "en-gb", new[] { attachment });

        var result = await _service.GetAttachmentAsync(1, pageGuid, attachGuid, "en-gb");
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.GUID, Is.EqualTo(attachGuid));
            Assert.That(result.Name, Is.EqualTo("report.pdf"));
            Assert.That(result.Extension, Is.EqualTo(".pdf"));
            Assert.That(result.MimeType, Is.EqualTo("application/pdf"));
            Assert.That(result.Size, Is.EqualTo(2048));
            Assert.That(result.Order, Is.EqualTo(0));
            Assert.That(result.Title, Is.EqualTo("report.pdf"));
            Assert.That(result.Description, Is.EqualTo("report.pdf"));
            Assert.That(result.LastModified, Is.EqualTo(lastModified));
        });
    }

    [Test]
    public async Task GetAttachmentAsync_WhenAttachmentGuidNotInList_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();
        var differentGuid = Guid.NewGuid();
        var attachment = CreateAttachment(differentGuid);

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", new[] { attachment });

        var result = await _service.GetAttachmentAsync(1, pageGuid, attachGuid, "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttachmentAsync_WhenListIsEmpty_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();

        SetupChannel(2, "emptychannel");
        SetupAttachments(pageGuid, "emptychannel", "en-gb", Array.Empty<BDO.ContentAttachment>());

        var result = await _service.GetAttachmentAsync(2, pageGuid, attachGuid, "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAttachmentAsync_WhenAssetIsNull_ReturnsOutputWithDefaultValues()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();
        var attachment = CreateAttachment(attachGuid, asset: null);

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", new[] { attachment });

        var result = await _service.GetAttachmentAsync(1, pageGuid, attachGuid, "en-gb");
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.GUID, Is.EqualTo(attachGuid));
            Assert.That(result.Name, Is.EqualTo(string.Empty));
            Assert.That(result.Extension, Is.EqualTo(string.Empty));
            Assert.That(result.MimeType, Is.EqualTo("application/octet-stream"));
            Assert.That(result.Size, Is.EqualTo(0));
            Assert.That(result.ImageWidth, Is.Null);
            Assert.That(result.ImageHeight, Is.Null);
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Description, Is.Null);
            Assert.That(result.LastModified, Is.Null);
            Assert.That(result.Order, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetAttachmentAsync_ForwardsChannelIdAndCultureToServices()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();

        SetupChannel(7, "channel7");
        SetupAttachments(pageGuid, "channel7", "fr-fr", Array.Empty<BDO.ContentAttachment>());

        await _service.GetAttachmentAsync(7, pageGuid, attachGuid, "fr-fr");

        _pageChannelResolverMock.Verify(x => x.GetChannelNameByPageGuidAsync(pageGuid, It.IsAny<CancellationToken>()), Times.Once);
        _pageAttachmentServiceMock.Verify(
            x => x.GetPageAttachmentsAsync(pageGuid, "channel7", "fr-fr", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAttachmentAsync_WithImageAsset_MapsWidthAndHeight()
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();
        var asset = CreateAsset("banner.png", ".png", 5120, width: 1920, height: 1080);
        var attachment = CreateAttachment(attachGuid, asset);

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", new[] { attachment });

        var result = await _service.GetAttachmentAsync(1, pageGuid, attachGuid, "en-gb");
        Assert.Multiple(() =>
        {
            Assert.That(result!.ImageWidth, Is.EqualTo(1920));
            Assert.That(result.ImageHeight, Is.EqualTo(1080));
        });
    }

    [Test]
    public async Task GetAttachmentAsync_PicksCorrectAttachment_WhenMultipleExist()
    {
        var pageGuid = Guid.NewGuid();
        var targetGuid = Guid.NewGuid();
        var asset = CreateAsset("target.pdf", ".pdf", 512);

        var attachments = new[]
        {
            CreateAttachment(Guid.NewGuid()),
            CreateAttachment(targetGuid, asset),
            CreateAttachment(Guid.NewGuid())
        };

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", attachments);

        var result = await _service.GetAttachmentAsync(1, pageGuid, targetGuid, "en-gb");
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.GUID, Is.EqualTo(targetGuid));
            Assert.That(result.Name, Is.EqualTo("target.pdf"));
        });
    }

    // ── GetAttachmentsAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetAttachmentsAsync_ReturnsAllMappedAttachments()
    {
        var pageGuid = Guid.NewGuid();
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        SetupChannel(3, "ch3");
        SetupAttachments(pageGuid, "ch3", "en-gb", new[]
        {
            CreateAttachment(guid1, CreateAsset("a.jpg", ".jpg", 100)),
            CreateAttachment(guid2, CreateAsset("b.png", ".png", 200))
        });

        var result = (await _service.GetAttachmentsAsync(3, pageGuid, "en-gb")).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].GUID, Is.EqualTo(guid1));
            Assert.That(result[1].GUID, Is.EqualTo(guid2));
        });
    }

    [Test]
    public async Task GetAttachmentsAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        var pageGuid = Guid.NewGuid();

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", Array.Empty<BDO.ContentAttachment>());

        var result = await _service.GetAttachmentsAsync(1, pageGuid, "en-gb");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAttachmentsAsync_ForwardsChannelIdAndCultureToServices()
    {
        var pageGuid = Guid.NewGuid();

        SetupChannel(5, "ch5");
        SetupAttachments(pageGuid, "ch5", "de-de", Array.Empty<BDO.ContentAttachment>());

        await _service.GetAttachmentsAsync(5, pageGuid, "de-de");

        _pageChannelResolverMock.Verify(x => x.GetChannelNameByPageGuidAsync(pageGuid, It.IsAny<CancellationToken>()), Times.Once);
        _pageAttachmentServiceMock.Verify(
            x => x.GetPageAttachmentsAsync(pageGuid, "ch5", "de-de", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAttachmentsAsync_WithNullAssets_ReturnsOutputsWithDefaultValues()
    {
        var pageGuid = Guid.NewGuid();
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", new[]
        {
            CreateAttachment(guid1, asset: null),
            CreateAttachment(guid2, asset: null)
        });

        var result = (await _service.GetAttachmentsAsync(1, pageGuid, "en-gb")).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo(string.Empty));
            Assert.That(result[0].MimeType, Is.EqualTo("application/octet-stream"));
            Assert.That(result[1].Name, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetAttachmentBinaryAsync_ForwardsArgumentsToPageAttachmentService()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();
        var expected = (Data: new byte[] { 1, 2, 3 }, ContentType: "application/pdf", FileName: "file.pdf");

        _pageChannelResolverMock
            .Setup(x => x.GetChannelNameByPageGuidAsync(pageGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync("ch9");

        _pageAttachmentServiceMock
            .Setup(x => x.GetPageAttachmentBinaryAsync(pageGuid, attachmentGuid, "ch9", "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.GetAttachmentBinaryAsync(9, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetAttachmentBinaryAsync_WhenChannelNameIsNull_ReturnsNull()
    {
        var pageGuid = Guid.NewGuid();
        var attachmentGuid = Guid.NewGuid();

        _pageChannelResolverMock
            .Setup(x => x.GetChannelNameByPageGuidAsync(pageGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _service.GetAttachmentBinaryAsync(9, pageGuid, attachmentGuid, "en-gb", CancellationToken.None);

        Assert.That(result, Is.Null);
        _pageAttachmentServiceMock.Verify(
            x => x.GetPageAttachmentBinaryAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── MimeType mapping ────────────────────────────────────────────────────────

    [TestCase(".png", "image/png")]
    [TestCase(".PNG", "image/png")]
    [TestCase(".jpg", "image/jpeg")]
    [TestCase(".jpeg", "image/jpeg")]
    [TestCase(".webp", "image/webp")]
    [TestCase(".gif", "image/gif")]
    [TestCase(".svg", "image/svg+xml")]
    [TestCase(".pdf", "application/pdf")]
    [TestCase(".docx", "application/octet-stream")]
    [TestCase(".txt", "application/octet-stream")]
    [TestCase(".zip", "application/octet-stream")]
    public async Task GetAttachmentAsync_MimeType_MappedCorrectlyForExtension(
        string extension, string expectedMimeType)
    {
        var pageGuid = Guid.NewGuid();
        var attachGuid = Guid.NewGuid();
        var asset = CreateAsset("file", extension, 100);
        var attachment = CreateAttachment(attachGuid, asset);

        SetupChannel(1, "ch");
        SetupAttachments(pageGuid, "ch", "en-gb", new[] { attachment });

        var result = await _service.GetAttachmentAsync(1, pageGuid, attachGuid, "en-gb");

        Assert.That(result!.MimeType, Is.EqualTo(expectedMimeType));
    }
}
