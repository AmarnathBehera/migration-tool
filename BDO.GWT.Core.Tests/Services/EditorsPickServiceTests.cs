using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class EditorsPickServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IEditorsPickCardService> _editorsPickCardServiceMock = null!;
    private EditorsPickService _service = null!;

    private sealed class TestPage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new WebPageFields();
    }

    [SetUp]
    public void SetUp()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _editorsPickCardServiceMock = new Mock<IEditorsPickCardService>();

        _service = new EditorsPickService(
            _filteringHelperMock.Object,
            _editorsPickCardServiceMock.Object);
    }

    private static EditorsPickData CreateData(
        bool filteringEnabled = true,
        string? seeAllText = "See all",
        string? seeAllUrl = "/editors-pick") => new()
    {
        Title = "Editor's Pick",
        Description = "Top content curated for you",
        FilteringEnabled = filteringEnabled,
        Filters = new HashSet<string> { "contentType", "businessLine" },
        EditorsPickGuids = Enumerable.Empty<ContentItemReference>(),
        FeaturedPageGuids = Enumerable.Empty<ContentItemReference>(),
        BackgroundImageUrl = "/media/bg.jpg",
        BackgroundImageAltText = "Background",
        SeeAllLinkText = seeAllText,
        SeeAllLinkUrl = seeAllUrl,
        SeeAllLinkURLTarget = "_blank",
        TopMargin = "20px",
        BottomMargin = "40px",
        WidgetID = "editors-pick-1"
    };

    private static PaginationDto<EditorsPickCardDto> EmptyPagination() => new()
    {
        Data = [],
        TotalCount = 0
    };

    // ── GetEditorsPickAsync ──────────────────────────────────────────────────

    [Test]
    public async Task GetEditorsPickAsync_WithAllData_MapsDtoCorrectly()
    {
        var currentPage = new TestPage();
        var data = CreateData();
        var cards = new[] { new EditorsPickCardDto { Title = "Card 1" } };

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _editorsPickCardServiceMock
            .Setup(x => x.CreateFiltersAsync(
                data.Filters,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<EditorsPickCardDto> { Data = cards, TotalCount = 1 });

        var result = await _service.GetEditorsPickAsync(currentPage, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Editor's Pick"));
            Assert.That(result.Description, Is.EqualTo("Top content curated for you"));
            Assert.That(result.BackgroundImageURL, Is.EqualTo("/media/bg.jpg"));
            Assert.That(result.BackgroundImageAltText, Is.EqualTo("Background"));
            Assert.That(result.FilteringTooltipEnabled, Is.True);
            Assert.That(result.TopMargin, Is.EqualTo("20px"));
            Assert.That(result.BottomMargin, Is.EqualTo("40px"));
            Assert.That(result.WidgetID, Is.EqualTo("editors-pick-1"));
            Assert.That(result.SeeAllLink, Is.Not.Null);
            Assert.That(result.SeeAllLink!.Title, Is.EqualTo("See all"));
            Assert.That(result.SeeAllLink.URL, Is.EqualTo("/editors-pick"));
            Assert.That(result.SeeAllLink.Target, Is.EqualTo("_blank"));
        });
    }

    [Test]
    public async Task GetEditorsPickAsync_WhenFilteringDisabled_DoesNotCreateFilters()
    {
        var currentPage = new TestPage();
        var data = CreateData(filteringEnabled: false);

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyPagination());

        var result = await _service.GetEditorsPickAsync(currentPage, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Filters, Is.Null);
            Assert.That(result.FilteringTooltipEnabled, Is.False);
        });

        _editorsPickCardServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetEditorsPickAsync_WhenSeeAllLinkTextIsNull_SetsSeeAllLinkToNull()
    {
        var currentPage = new TestPage();
        var data = CreateData(seeAllText: null);

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _editorsPickCardServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyPagination());

        var result = await _service.GetEditorsPickAsync(currentPage, data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetEditorsPickAsync_WhenSeeAllUrlIsNull_SetsSeeAllLinkToNull()
    {
        var currentPage = new TestPage();
        var data = CreateData(seeAllUrl: null);

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _editorsPickCardServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyPagination());

        var result = await _service.GetEditorsPickAsync(currentPage, data, CancellationToken.None);

        Assert.That(result.SeeAllLink, Is.Null);
    }

    [Test]
    public void GetEditorsPickAsync_ThrowsArgumentNullException_WhenCurrentPageIsNull()
    {
        var data = CreateData();

        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetEditorsPickAsync<TestPage>(null!, data, CancellationToken.None));
    }

    [Test]
    public void GetEditorsPickAsync_ThrowsArgumentNullException_WhenDataIsNull()
    {
        var currentPage = new TestPage();

        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetEditorsPickAsync(currentPage, null!, CancellationToken.None));
    }

    // ── GetFilteredItemsAsync ────────────────────────────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_FeaturedGuids_AreIncludedInQuery()
    {
        var featuredGuid = Guid.NewGuid();
        var selectedGuid = Guid.NewGuid();

        var featuredRef = new ContentItemReference { Identifier = featuredGuid };
        var selectedRef = new ContentItemReference { Identifier = selectedGuid };

        HashSet<Guid>? capturedGuids = null;

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, HashSet<Guid>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (_, guids, _, _) => capturedGuids = guids)
            .ReturnsAsync(EmptyPagination());

        await _service.GetFilteredItemsAsync(
            new HashSet<ContentItemReference> { featuredRef },
            new HashSet<ContentItemReference> { selectedRef },
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedGuids, Is.Not.Null);
            Assert.That(capturedGuids, Contains.Item(featuredGuid));
            Assert.That(capturedGuids, Contains.Item(selectedGuid));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_IsFeaturedPredicate_ReturnsTrueOnlyForFeaturedPages()
    {
        var featuredGuid = Guid.NewGuid();
        var selectedGuid = Guid.NewGuid();

        var featuredRef = new ContentItemReference { Identifier = featuredGuid };
        var selectedRef = new ContentItemReference { Identifier = selectedGuid };

        Func<IWebPageFieldsSource, bool>? capturedPredicate = null;

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, HashSet<Guid>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (_, _, predicate, _) => capturedPredicate = predicate)
            .ReturnsAsync(EmptyPagination());

        await _service.GetFilteredItemsAsync(
            new HashSet<ContentItemReference> { featuredRef },
            new HashSet<ContentItemReference> { selectedRef },
            new CardsQuery(),
            CancellationToken.None);

        var featuredPage = new Mock<IWebPageFieldsSource>();
        featuredPage.Setup(x => x.SystemFields)
            .Returns(new WebPageFields { ContentItemGUID = featuredGuid });

        var regularPage = new Mock<IWebPageFieldsSource>();
        regularPage.Setup(x => x.SystemFields)
            .Returns(new WebPageFields { ContentItemGUID = selectedGuid });

        Assert.Multiple(() =>
        {
            Assert.That(capturedPredicate, Is.Not.Null);
            Assert.That(capturedPredicate!(featuredPage.Object), Is.True);
            Assert.That(capturedPredicate!(regularPage.Object), Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_NullFeaturedGuids_TreatedAsEmpty()
    {
        var selectedGuid = Guid.NewGuid();
        var selectedRef = new ContentItemReference { Identifier = selectedGuid };

        HashSet<Guid>? capturedGuids = null;

        _editorsPickCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<HashSet<Guid>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, HashSet<Guid>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (_, guids, _, _) => capturedGuids = guids)
            .ReturnsAsync(EmptyPagination());

        await _service.GetFilteredItemsAsync(
            null!,
            new HashSet<ContentItemReference> { selectedRef },
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedGuids, Is.Not.Null);
            Assert.That(capturedGuids, Contains.Item(selectedGuid));
            Assert.That(capturedGuids, Has.Count.EqualTo(1));
        });
    }
}
