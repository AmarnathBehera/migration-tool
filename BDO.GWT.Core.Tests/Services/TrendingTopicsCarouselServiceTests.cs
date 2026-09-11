using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Reflection;

namespace BDO.GWT.Core.Tests.Services;

public class TrendingTopicsCarouselServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private TrendingTopicsCarouselService _service = null!;
    private Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>> _originalRetriever = null!;
    private int _retrieveCallCount;

    // -----------------------------------------------------------------------
    // Setup / Teardown
    // -----------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _cultureContextMock = new Mock<ICultureContext>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _retrieveCallCount = 0;

        _cultureContextMock
            .SetupGet(x => x.CurrentLanguageTag)
            .Returns("en-gb");

        _service = new TrendingTopicsCarouselService(
            _contentRetrieverMock.Object,
            _cultureContextMock.Object,
            _webPageUrlRetrieverMock.Object);

        // Capture the real delegate so TearDown can restore it.
        var field = RetrieveField();
        _originalRetriever = (Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>>)
            field.GetValue(_service)!;

        // Default stub: return nothing and count calls.
        SetRetriever((_, _, _, _) =>
        {
            _retrieveCallCount++;
            return Task.FromResult(Enumerable.Empty<IWebPageFieldsSource>());
        });
    }

    [TearDown]
    public void TearDown()
    {
        RetrieveField().SetValue(_service, _originalRetriever);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static FieldInfo RetrieveField() =>
        typeof(TrendingTopicsCarouselService)
            .GetField("_retrieveContentByGuidsAsync", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Field _retrieveContentByGuidsAsync not found.");

    private void SetRetriever(
        Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>> func)
        => RetrieveField().SetValue(_service, func);

    private static TrendingTopicsCarouselData CreateData(IEnumerable<ContentItemReference>? pages = null) => new()
    {
        Title = "Title",
        Description = "Description",
        TrendingTopicsPages = pages ?? Enumerable.Empty<ContentItemReference>(),
        LearnMoreButtonURLTarget = "_blank",
        TopMargin = "20px",
        BottomMargin = "40px",
        WidgetID = "widget-1"
    };

    private static ContentItemReference Ref(Guid guid) => new() { Identifier = guid };

    /// <summary>Implements both IWebPageFieldsSource and IMetadata — the "happy path" page.</summary>
    private sealed class PageWithMetadata : IWebPageFieldsSource, IMetadata
    {
        public WebPageFields SystemFields { get; set; } = new()
        {
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "Page Title"
        };

        public string MetadataTitle { get; set; } = "Meta Title";
        public string MetadataDescription { get; set; } = "<p>Meta Description</p>";
        public IEnumerable<ContentAttachment> MetadataTeaserImage { get; set; } = [];
        public string MetadataTeaserImageAltText { get; set; } = "Alt text";
        public string MetadataCanonical { get; set; } = string.Empty;
    }

    /// <summary>Implements IWebPageFieldsSource only — filtered out because it lacks IMetadata.</summary>
    private sealed class PageWithoutMetadata : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new()
        {
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "No Metadata"
        };
    }

    /// <summary>Happy-path page that also carries a teaser image attachment.</summary>
    private sealed class PageWithAttachment : IWebPageFieldsSource, IMetadata
    {
        public WebPageFields SystemFields { get; set; } = new()
        {
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "With Image"
        };

        public string MetadataTitle { get; set; } = "Title";
        public string MetadataDescription { get; set; } = "Description";
        public IEnumerable<ContentAttachment> MetadataTeaserImage { get; set; } =
        [
            new ContentAttachment
            {
                ContentAttachmentAsset = new ContentItemAsset { Url = "/image.jpg" }
            }
        ];
        public string MetadataTeaserImageAltText { get; set; } = "Image alt";
        public string MetadataCanonical { get; set; } = string.Empty;
    }

    // -----------------------------------------------------------------------
    // 1. Top-level DTO field mapping
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_MapsTopLevelFields()
    {
        var result = await _service.GetTrendingTopicsCarouselAsync(CreateData(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Title"));
            Assert.That(result.Description, Is.EqualTo("Description"));
            Assert.That(result.TopMargin, Is.EqualTo("20px"));
            Assert.That(result.BottomMargin, Is.EqualTo("40px"));
            Assert.That(result.WidgetID, Is.EqualTo("widget-1"));
            Assert.That(result.LearnMoreButtonURLTarget, Is.EqualTo("_blank"));
            Assert.That(result.Items, Is.Empty);
        });
    }

    // -----------------------------------------------------------------------
    // 2. Retriever not called when no pages are provided
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_NoPages_DoesNotInvokeRetriever()
    {
        await _service.GetTrendingTopicsCarouselAsync(CreateData(), CancellationToken.None);

        Assert.That(_retrieveCallCount, Is.EqualTo(0));
    }

    // -----------------------------------------------------------------------
    // 3. Correct GUIDs forwarded to retriever
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_WithOnePage_ForwardsCorrectGuid()
    {
        var guid = Guid.NewGuid();
        var captured = new List<Guid>();

        SetRetriever((_, guids, _, _) =>
        {
            captured.AddRange(guids);
            return Task.FromResult<IEnumerable<IWebPageFieldsSource>>([new PageWithMetadata()]);
        });

        // IWebPageUrlRetriever.Retrieve(IWebPageFieldsSource, string, CancellationToken)
        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        await _service.GetTrendingTopicsCarouselAsync(CreateData([Ref(guid)]), CancellationToken.None);

        Assert.That(captured, Is.EquivalentTo(new[] { guid }));
    }

    // -----------------------------------------------------------------------
    // 4. Pages without IMetadata are excluded from results
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_IgnoresPagesWithoutMetadata()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithoutMetadata()];
            return Task.FromResult(pages);
        });

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    // -----------------------------------------------------------------------
    // 5. Mixed pages: only those with IMetadata produce items
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_MixedPages_OnlyMetadataPagesProduceItems()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithMetadata(), new PageWithoutMetadata()];
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid()), Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    // -----------------------------------------------------------------------
    // 6. Item DTO fields mapped correctly (image, URL, alt text)
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_MapsItemFields_ImageAndUrl()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithAttachment()];
            return Task.FromResult(pages);
        });


        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/topics/topic-1", "/topics/topic-1"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);
        var item = result.Items.Single();

        Assert.Multiple(() =>
        {

            Assert.That(item.Description, Is.EqualTo("Description"));
            Assert.That(item.ReadMoreLink, Is.EqualTo("/topics/topic-1"));
            Assert.That(item.ImageURL, Is.EqualTo("/image.jpg"));
            Assert.That(item.ImageAltText, Is.EqualTo("Image alt"));
            Assert.That(item.IsFeatured, Is.False);
        });
    }

    // -----------------------------------------------------------------------
    // 7. Culture context language tag is forwarded to IWebPageUrlRetriever
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_UsesCultureContextForUrlRetrieval()
    {
        _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns("fr-fr");

        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithAttachment()];
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), "fr-fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/fr/page", "/fr/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.Single().ReadMoreLink, Is.EqualTo("/fr/page"));
    }

    // -----------------------------------------------------------------------
    // 8. CancellationToken is forwarded to the retriever delegate
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_PreservesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        CancellationToken receivedToken = default;

        SetRetriever((_, _, _, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult<IEnumerable<IWebPageFieldsSource>>([new PageWithMetadata()]);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), token))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        await _service.GetTrendingTopicsCarouselAsync(CreateData([Ref(Guid.NewGuid())]), token);

        Assert.That(receivedToken, Is.EqualTo(token));
    }

    // -----------------------------------------------------------------------
    // 9. Multiple pages all produce items
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_ProcessesMultiplePages()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithAttachment(), new PageWithMetadata()];
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid()), Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.Count(), Is.EqualTo(2));
    }

    // -----------------------------------------------------------------------
    // 10. Empty page collection returns empty Items without calling retriever
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_EmptyPageCollection_ReturnsEmptyItems()
    {
        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData(Array.Empty<ContentItemReference>()), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Empty);
            Assert.That(_retrieveCallCount, Is.EqualTo(0));
        });
    }

    // -----------------------------------------------------------------------
    // 11. IsFeatured is always false (no featured logic in service)
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_IsFeatured_IsAlwaysFalse()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages =
            [
                new PageWithAttachment(),
                new PageWithMetadata()
            ];
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid()), Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.All(i => i.IsFeatured == false), Is.True);
    }

    // -----------------------------------------------------------------------
    // 12. Page with no teaser image yields null ImageURL
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_NoTeaserImage_ImageUrlIsNull()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithMetadata()]; // MetadataTeaserImage = []
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.Single().ImageURL, Is.Null);
    }

    // -----------------------------------------------------------------------
    // 13. HTML stripped from MetadataDescription
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_StripHtmlFromDescription()
    {
        var page = new PageWithMetadata
        {
            MetadataDescription = "<p>Plain <strong>text</strong></p>"
        };

        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [page];
            return Task.FromResult(pages);
        });

        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/page", "/page"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);

        // HtmlHelper.ExtractTextFromHtml should strip all tags
        Assert.That(result.Items.Single().Description, Does.Not.Contain("<p>"));
        Assert.That(result.Items.Single().Description, Does.Not.Contain("<strong>"));
    }

    // -----------------------------------------------------------------------
    // 14. Tilde (~) prefix is stripped from ReadMoreLink via WebPageUrl.RelativePath
    // -----------------------------------------------------------------------

    [Test]
    public async Task GetTrendingTopicsCarouselAsync_ReadMoreLink_UsesRelativePath_NotAbsolutePath()
    {
        SetRetriever((_, _, _, _) =>
        {
            IEnumerable<IWebPageFieldsSource> pages = [new PageWithMetadata()];
            return Task.FromResult(pages);
        });

        // WebPageUrl ctor: (absolutePath, relativePath) — RelativePath is the second arg
        _webPageUrlRetrieverMock
            .Setup(x => x.Retrieve(It.IsAny<IWebPageFieldsSource>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/insights/article", "/insights/article"));

        var result = await _service.GetTrendingTopicsCarouselAsync(
            CreateData([Ref(Guid.NewGuid())]), CancellationToken.None);

        Assert.That(result.Items.Single().ReadMoreLink, Is.EqualTo("/insights/article"));
        Assert.That(result.Items.Single().ReadMoreLink, Does.Not.StartWith("~"));
    }


}