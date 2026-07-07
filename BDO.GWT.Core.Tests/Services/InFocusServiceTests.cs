using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InFocusServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private InFocusService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();

        _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns("en-GB");

        _service = new InFocusService(
            _contentServiceMock.Object,
            _cultureContextMock.Object);
    }

    private static InFocusPage CreateInFocusPage(
        Guid contentItemGuid,
        string? metadataTitle = null,
        string? titleLink1 = null,
        string? summaryLink1 = null,
        string? selectorLink1 = null,
        string? textLink1 = null,
        bool newTabLink1 = false,
        string? titleLink2 = null,
        string? summaryLink2 = null,
        string? selectorLink2 = null,
        string? textLink2 = null,
        bool newTabLink2 = false,
        string? titleLink3 = null,
        string? summaryLink3 = null,
        string? selectorLink3 = null,
        string? textLink3 = null,
        bool newTabLink3 = false)
    {
        var page = new InFocusPage
        {
            SystemFields = new WebPageFields { ContentItemGUID = contentItemGuid },
            MetadataTitle = metadataTitle,
            TitleLink1 = titleLink1,
            SummaryLink1 = summaryLink1,
            SelectorLink1 = selectorLink1,
            TextLink1 = textLink1,
            NewTabLink1 = newTabLink1,
            TitleLink2 = titleLink2,
            SummaryLink2 = summaryLink2,
            SelectorLink2 = selectorLink2,
            TextLink2 = textLink2,
            NewTabLink2 = newTabLink2,
            TitleLink3 = titleLink3,
            SummaryLink3 = summaryLink3,
            SelectorLink3 = selectorLink3,
            TextLink3 = textLink3,
            NewTabLink3 = newTabLink3,
        };

        return page;
    }

    private void SetupGetPagesAsync(IEnumerable<InFocusPage> pages)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InFocusPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);
    }

    #region GetInFocusAsync - Basic mapping

    [Test]
    public async Task GetInFocusAsync_MapsDataFieldsToDto()
    {
        SetupGetPagesAsync([]);

        var data = new InFocusData
        {
            Title = "Focus Title",
            Subtitle = "Focus Subtitle",
            ImageURL = "https://example.com/image.jpg",
            ImageAltText = "Alt text",
            PageGuids = [],
            TopMargin = "20px",
            BottomMargin = "40px",
            WidgetID = "in-focus-widget"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Focus Title"));
            Assert.That(result.Subtitle, Is.EqualTo("Focus Subtitle"));
            Assert.That(result.ImageURL, Is.EqualTo("https://example.com/image.jpg"));
            Assert.That(result.ImageAltText, Is.EqualTo("Alt text"));
            Assert.That(result.TopMargin, Is.EqualTo("20px"));
            Assert.That(result.BottomMargin, Is.EqualTo("40px"));
            Assert.That(result.WidgetID, Is.EqualTo("in-focus-widget"));
        });
    }

    [Test]
    public async Task GetInFocusAsync_WithNoPages_ReturnsEmptyTopics()
    {
        SetupGetPagesAsync([]);

        var data = new InFocusData
        {
            Title = "Title",
            PageGuids = [],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Topics, Is.Empty);
    }

    #endregion

    #region GetInFocusAsync - Topics mapping

    [Test]
    public async Task GetInFocusAsync_MapsPageMetadataTitleToTopicTitle()
    {
        var guid = Guid.NewGuid();
        var page = CreateInFocusPage(guid, metadataTitle: "Topic Title");

        SetupGetPagesAsync([page]);

        var data = new InFocusData
        {
            PageGuids = [guid],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Topics, Has.Length.EqualTo(1));
        Assert.That(result.Topics[0].Title, Is.EqualTo("Topic Title"));
    }

    [Test]
    public async Task GetInFocusAsync_OrdersTopicsByPageGuidsOrder()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();

        var page1 = CreateInFocusPage(guid1, metadataTitle: "First");
        var page2 = CreateInFocusPage(guid2, metadataTitle: "Second");
        var page3 = CreateInFocusPage(guid3, metadataTitle: "Third");

        // Return pages in different order than requested
        SetupGetPagesAsync([page3, page1, page2]);

        var data = new InFocusData
        {
            PageGuids = [guid1, guid2, guid3],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Topics, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(result.Topics[0].Title, Is.EqualTo("First"));
            Assert.That(result.Topics[1].Title, Is.EqualTo("Second"));
            Assert.That(result.Topics[2].Title, Is.EqualTo("Third"));
        });
    }

    #endregion

    #region GetInFocusAsync - Items mapping

    [Test]
    public async Task GetInFocusAsync_MapsAllThreeLinksToItems()
    {
        var guid = Guid.NewGuid();
        var page = CreateInFocusPage(guid,
            titleLink1: "Title 1", summaryLink1: "Summary 1", selectorLink1: "/link1", textLink1: "Read more 1", newTabLink1: true,
            titleLink2: "Title 2", summaryLink2: "Summary 2", selectorLink2: "/link2", textLink2: "Read more 2", newTabLink2: false,
            titleLink3: "Title 3", summaryLink3: "Summary 3", selectorLink3: "/link3", textLink3: "Read more 3", newTabLink3: true);

        SetupGetPagesAsync([page]);

        var data = new InFocusData
        {
            PageGuids = [guid],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        var items = result.Topics[0].Items;
        Assert.That(items, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Title, Is.EqualTo("Title 1"));
            Assert.That(items[0].Summary, Is.EqualTo("Summary 1"));
            Assert.That(items[0].Link, Is.EqualTo("/link1"));
            Assert.That(items[0].LinkText, Is.EqualTo("Read more 1"));
            Assert.That(items[0].OpenInNewTab, Is.True);

            Assert.That(items[1].Title, Is.EqualTo("Title 2"));
            Assert.That(items[1].Summary, Is.EqualTo("Summary 2"));
            Assert.That(items[1].Link, Is.EqualTo("/link2"));
            Assert.That(items[1].LinkText, Is.EqualTo("Read more 2"));
            Assert.That(items[1].OpenInNewTab, Is.False);

            Assert.That(items[2].Title, Is.EqualTo("Title 3"));
            Assert.That(items[2].Summary, Is.EqualTo("Summary 3"));
            Assert.That(items[2].Link, Is.EqualTo("/link3"));
            Assert.That(items[2].LinkText, Is.EqualTo("Read more 3"));
            Assert.That(items[2].OpenInNewTab, Is.True);
        });
    }

    [Test]
    public async Task GetInFocusAsync_SkipsItemsWithAllEmptyFields()
    {
        var guid = Guid.NewGuid();
        var page = CreateInFocusPage(guid,
            titleLink1: "Title 1", summaryLink1: "Summary 1", selectorLink1: "/link1", textLink1: "Read more",
            titleLink2: null, summaryLink2: null, selectorLink2: null,
            titleLink3: "Title 3", summaryLink3: "Summary 3", selectorLink3: "/link3", textLink3: "More");

        SetupGetPagesAsync([page]);

        var data = new InFocusData
        {
            PageGuids = [guid],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        var items = result.Topics[0].Items;
        Assert.That(items, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Title, Is.EqualTo("Title 1"));
            Assert.That(items[1].Title, Is.EqualTo("Title 3"));
        });
    }

    [Test]
    public async Task GetInFocusAsync_IncludesItemWhenOnlyTitleIsSet()
    {
        var guid = Guid.NewGuid();
        var page = CreateInFocusPage(guid,
            titleLink1: "Only Title", summaryLink1: null, selectorLink1: null);

        SetupGetPagesAsync([page]);

        var data = new InFocusData
        {
            PageGuids = [guid],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Topics[0].Items, Has.Length.EqualTo(1));
        Assert.That(result.Topics[0].Items[0].Title, Is.EqualTo("Only Title"));
    }

    [Test]
    public async Task GetInFocusAsync_IncludesItemWhenOnlyLinkIsSet()
    {
        var guid = Guid.NewGuid();
        var page = CreateInFocusPage(guid,
            titleLink1: null, summaryLink1: null, selectorLink1: "/some-link");

        SetupGetPagesAsync([page]);

        var data = new InFocusData
        {
            PageGuids = [guid],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Topics[0].Items, Has.Length.EqualTo(1));
        Assert.That(result.Topics[0].Items[0].Link, Is.EqualTo("/some-link"));
    }

    #endregion

    #region GetInFocusAsync - Service interaction

    [Test]
    public async Task GetInFocusAsync_PassesCancellationTokenToContentService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        CancellationToken capturedToken = default;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InFocusPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (_, _, _, ct) => capturedToken = ct)
            .ReturnsAsync(Enumerable.Empty<InFocusPage>());

        var data = new InFocusData
        {
            PageGuids = [Guid.NewGuid()],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        await _service.GetInFocusAsync(data, token);

        Assert.That(capturedToken, Is.EqualTo(token));
    }

    [Test]
    public async Task GetInFocusAsync_UsesCurrentLanguageTagFromCultureContext()
    {
        _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns("fr-FR");

        RetrievePagesParameters? capturedParams = null;
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<InFocusPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedParams = p)
            .ReturnsAsync(Enumerable.Empty<InFocusPage>());

        var data = new InFocusData
        {
            PageGuids = [Guid.NewGuid()],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(capturedParams, Is.Not.Null);
        Assert.That(capturedParams!.LanguageName, Is.EqualTo("fr-FR"));
    }

    [Test]
    public async Task GetInFocusAsync_CallsGetPagesAsyncExactlyOnce()
    {
        SetupGetPagesAsync([]);

        var data = new InFocusData
        {
            PageGuids = [Guid.NewGuid()],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        await _service.GetInFocusAsync(data, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<InFocusPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetInFocusAsync - Null/empty data

    [Test]
    public async Task GetInFocusAsync_WithNullTitle_ReturnsDtoWithNullTitle()
    {
        SetupGetPagesAsync([]);

        var data = new InFocusData
        {
            Title = null,
            PageGuids = [],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.Title, Is.Null);
    }

    [Test]
    public async Task GetInFocusAsync_WithNullImageURL_ReturnsDtoWithNullImageURL()
    {
        SetupGetPagesAsync([]);

        var data = new InFocusData
        {
            ImageURL = null,
            PageGuids = [],
            TopMargin = "0px",
            BottomMargin = "0px"
        };

        var result = await _service.GetInFocusAsync(data, CancellationToken.None);

        Assert.That(result.ImageURL, Is.Null);
    }

    #endregion
}
