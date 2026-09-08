using BDO;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Data;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCarouselServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IInsightCardService> _insightCardServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IInsightCarouselStateStore> _stateStoreMock = null!;
    private InsightCarouselService _service = null!;

    [SetUp]
    public void Setup()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _insightCardServiceMock = new Mock<IInsightCardService>();
        _contentServiceMock = new Mock<IContentService>();
        _stateStoreMock = new Mock<IInsightCarouselStateStore>();

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("en-us");

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0,
            });

        _service = new InsightCarouselService(
            _filteringHelperMock.Object,
            _insightCardServiceMock.Object,
            _contentServiceMock.Object,
            _stateStoreMock.Object);
    }

    private sealed class DummyPage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new() { ContentItemGUID = Guid.NewGuid() };
    }

    private sealed class businessLine : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = new() { ContentItemGUID = Guid.NewGuid() };
    }

    [Test]
    public void GetInsightCarouselAsync_Throws_WhenArgumentsAreNull()
    {
        var page = new DummyPage();
        var data = new InsightCarouselData();

        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetInsightCarouselAsync<DummyPage>(null!, data, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetInsightCarouselAsync(page, null!, CancellationToken.None));
        });
    }

    [Test]
    public async Task GetInsightCarouselAsync_FilteringEnabled_PopulatesDto_AndStoresState()
    {
        var page = new DummyPage();
        var pageCard = new ContentPageCardDto { Title = "item-1" };
        HashSet<ContentItemReference>? storedFeatured = null;

        _stateStoreMock
            .Setup(x => x.Save(It.IsAny<Guid>(), It.IsAny<HashSet<ContentItemReference>>()))
            .Callback<Guid, HashSet<ContentItemReference>>((_, featured) => storedFeatured = featured);

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new FilterItemDto<OptionDto>
                {
                    Name = "businessLine",
                    Options = [],
                },
            ]);

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = [pageCard],
                TotalCount = 1,
            });

        var featuredGuid = Guid.NewGuid();
        var data = new InsightCarouselData
        {
            Title = "Title",
            Description = "Description",
            FilteringEnabled = true,
            Filters = ["businessLine"],
            FeaturedInsightNodeGuids = [new ContentItemReference { Identifier = featuredGuid }],
            SeeAllLinkText = "See all",
            SeeAllLinkUrl = "/insights",
            SeeAllLinkURLTarget = null,
        };

        var dto = await _service.GetInsightCarouselAsync(page, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("Title"));
            Assert.That(dto.Description, Is.EqualTo("Description"));
            Assert.That(dto.Filters, Is.Not.Null);
            Assert.That(dto.FilteringTooltipEnabled, Is.True);
            Assert.That(dto.Items!.Single().Title, Is.EqualTo("item-1"));
            Assert.That(dto.SeeAllLink, Is.Not.Null);
            Assert.That(dto.SeeAllLink!.Target, Is.EqualTo("_self"));
            Assert.That(dto.CurrentPageGuid, Is.EqualTo(page.SystemFields.ContentItemGUID));
            Assert.That(dto.PageType, Is.EqualTo(nameof(DummyPage)));
            Assert.That(dto.StateGUID, Is.Not.EqualTo(Guid.Empty));
            Assert.That(storedFeatured, Is.Not.Null);
            Assert.That(storedFeatured!.Any(x => x.Identifier == featuredGuid), Is.True);
        });

        _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInsightCarouselAsync_FilteringDisabled_DoesNotCreateFilters_AndKeepsSeeAllLinkNull()
    {
        var page = new DummyPage();

        var dto = await _service.GetInsightCarouselAsync(page, new InsightCarouselData { FilteringEnabled = false }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dto.Filters, Is.Null);
            Assert.That(dto.SeeAllLink, Is.Null);
        });

        _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetFilteredItemsAsync_KnownFilterType_UsesDecoratorAndFilteredDefaults()
    {
        var page = new businessLine();
        var featuredGuid = Guid.NewGuid();

        CardsQuery? capturedQuery = null;
        Action<RetrievePagesQueryParameters>? capturedDecorator = null;
        Func<Insight, bool>? capturedFeaturedPredicate = null;
        string? capturedLanguage = null;

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((query, _, language, decorateQuery, featured, _) =>
            {
                capturedQuery = query;
                capturedDecorator = decorateQuery;
                capturedFeaturedPredicate = featured;
                capturedLanguage = language;
            })
            .ReturnsAsync((PaginationDto<ContentPageCardDto>)null!);

        var result = await _service.GetFilteredItemsAsync(
            page,
            [new ContentItemReference { Identifier = featuredGuid }],
            new CardsQuery(),
            CancellationToken.None);

        var hit = capturedFeaturedPredicate!(new Insight { SystemFields = new WebPageFields { ContentItemGUID = featuredGuid } });
        var miss = capturedFeaturedPredicate!(new Insight { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } });

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(1));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(20));
            Assert.That(capturedDecorator, Is.Not.Null);
            Assert.That(capturedLanguage, Is.EqualTo("en-us"));
            Assert.That(hit, Is.True);
            Assert.That(miss, Is.False);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_UnknownType_UsesUnfilteredDefaultsAndNoDecorator()
    {
        var page = new DummyPage();
        CardsQuery? capturedQuery = null;
        Action<RetrievePagesQueryParameters>? capturedDecorator = null;

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((query, _, _, decorateQuery, _, _) =>
            {
                capturedQuery = query;
                capturedDecorator = decorateQuery;
            })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = [],
                TotalCount = 0,
            });

        await _service.GetFilteredItemsAsync(page, null!, null!, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(1));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(10));
            Assert.That(capturedDecorator, Is.Null);
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_UsesStateStore_WhenStateIsProvided()
    {
        var state = Guid.NewGuid();
        var featuredGuid = Guid.NewGuid();
        Func<Insight, bool>? capturedFeaturedPredicate = null;

        _stateStoreMock
            .Setup(x => x.TryGet(state, out It.Ref<HashSet<ContentItemReference>>.IsAny))
            .Returns((Guid _, out HashSet<ContentItemReference> featured) =>
            {
                featured = [new ContentItemReference { Identifier = featuredGuid }];
                return true;
            });

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((_, _, _, _, featured, _) =>
            {
                capturedFeaturedPredicate = featured;
            })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = [new ContentPageCardDto()],
                TotalCount = 1,
            });

        await _service.GetAPIFilteredItemsAsync(
            new CardsQuery
            {
                State = state,
                PageType = nameof(businessLine),
                CurrentPageGuid = Guid.NewGuid(),
                CurrentPage = null,
                PageSize = null,
            },
            "fr-fr",
            CancellationToken.None);

        var isFeatured = capturedFeaturedPredicate!(new Insight { SystemFields = new WebPageFields { ContentItemGUID = featuredGuid } });

        Assert.That(isFeatured, Is.True);
        _stateStoreMock.Verify(x => x.TryGet(state, out It.Ref<HashSet<ContentItemReference>>.IsAny), Times.Once);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_Throws_WhenQueryIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetAPIFilteredItemsAsync(null!, "en-us", CancellationToken.None));
    }
}
