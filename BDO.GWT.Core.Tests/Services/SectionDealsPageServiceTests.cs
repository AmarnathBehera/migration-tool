using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class SectionDealsPageServiceTests
{
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IDealCardsService> _dealCardsServiceMock = null!;
    private SectionDealsPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cultureContextMock = new Mock<ICultureContext>();
        _contentServiceMock = new Mock<IContentService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _dealCardsServiceMock = new Mock<IDealCardsService>();

        _cultureContextMock
            .Setup(c => c.CurrentLanguageTag)
            .Returns("en-us");

        _websiteChannelContextMock
            .Setup(c => c.WebsiteChannelID)
            .Returns(1);

        _service = new SectionDealsPageService(
            _cultureContextMock.Object,
            _contentServiceMock.Object,
            _websiteChannelContextMock.Object,
            _dealCardsServiceMock.Object);
    }

    // ── Builders ────────────────────────────────────────────────────────────

    private static SectionDeals CreateDefaultSectionDealsPage(
        bool isGlobal = false,
        bool filteringEnabled = false,
        string? globalFilters = null,
        string? sectionFilters = null) => new SectionDeals
        {
            IsGlobal = isGlobal,
            SectionDealsFilteringEnabled = filteringEnabled,
            GlobalSectionDealsFilters = globalFilters,
            SectionDealsFilters = sectionFilters,
            SystemFields = new WebPageFields
            {
                ContentItemID = 100,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "Section Deals",
                WebPageItemID = 200,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = "Section Deals Page",
                WebPageUrlPath = "/deals",
                WebPageItemTreePath = "/deals",
            },
        };

    /// <summary>
    /// Mocks RetrieveCurrentPageAsync — used by GetSectionDealsDtoAsync.
    /// </summary>
    private void SetupCurrentPage(SectionDeals? page) =>
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<SectionDeals>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

    /// <summary>
    /// Mocks GetPagesAsync — used by GetSectionDealsDealPaginationDtoAsync.
    /// </summary>
    private void SetupSectionDealsPages(IEnumerable<SectionDeals> pages) =>
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<SectionDeals>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);

    // ── GetSectionDealsDtoAsync ─────────────────────────────────────────────

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenNoPageFound_ReturnsEmptyDto()
    {
        SetupCurrentPage(null);

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Filters, Is.Empty);
            Assert.That(result.PageSize, Is.EqualTo(12));
            Assert.That(result.ApiEndpoint, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenPageExists_ReturnsDefaultPageSize12()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.PageSize, Is.EqualTo(12));
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenPageExists_SetsApiEndpointWithContentItemGuid()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupCurrentPage(page);

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.ApiEndpoint, Is.EqualTo($"Deals/{page.SystemFields.ContentItemGUID}/Get"));
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_CallsRetrieveCurrentPageWithLinkedItemsMaxLevel5()
    {
        SetupCurrentPage(null);

        await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<SectionDeals>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenFilteringDisabled_ReturnsEmptyFiltersList()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(filteringEnabled: false));

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters, Is.Empty);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenIsGlobal_UsesGlobalSectionDealsFilters()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: true,
            filteringEnabled: true,
            globalFilters: "dealType;industry",
            sectionFilters: "country;year"));

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardDealType>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardIndustry>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardIndustry>());

        await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        // DealCardDealType and DealCardIndustry come from globalFilters, not sectionFilters
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenNotGlobal_UsesSectionDealsFilters()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            globalFilters: "dealType;industry",
            sectionFilters: "country;year"));

        _contentServiceMock
            .Setup(x => x.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        // SectionDealsFilters ("country;year") are used, not globalFilters
        Assert.That(result.Filters, Is.Not.Null);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_FiltersAreSemicolonSeparated()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: "dealType;industry;country"));

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardDealType>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardIndustry>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardIndustry>());

        _contentServiceMock
            .Setup(x => x.GetItemsByTypesAsync(
                It.IsAny<Type[]>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters?.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_EmptyFilterString_YieldsEmptyFilterSet()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: ""));

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters, Is.Empty);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhitespaceOnlyFilterString_YieldsEmptyFilterSet()
    {
        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: "   "));

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters, Is.Empty);
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenLanguageIsGB_RemapsFiltersToStandard()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-gb");

        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: $"{FilterParameterName.DealType};{FilterParameterName.Industry};{FilterParameterName.SubIndustry}"));

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardDealType>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardIndustry>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardIndustry>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardSubIndustry>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardSubIndustry>());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters?.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_WhenLanguageIsNotGB_UsesOriginalFilterSet()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("de-de");

        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: $"{FilterParameterName.DealType};{FilterParameterName.Industry}"));

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardDealType>());

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardIndustry>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardIndustry>());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters, Is.Not.Null);
        Assert.That(result.Filters?.Count(), Is.GreaterThan(0));
    }

    [Test]
    public async Task GetSectionDealsDtoAsync_InvalidLanguageTag_TreatsAsNonGB()
    {
        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("invalid-tag");

        SetupCurrentPage(CreateDefaultSectionDealsPage(
            isGlobal: false,
            filteringEnabled: true,
            sectionFilters: FilterParameterName.DealType));

        _contentServiceMock
            .Setup(x => x.GetPagesAsync<DealCardDealType>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DealCardDealType>());

        var result = await _service.GetSectionDealsDtoAsync(CancellationToken.None);

        Assert.That(result.Filters, Is.Not.Null);
        Assert.That(result.Filters?.Count(), Is.EqualTo(1));
    }

    // ── GetSectionDealsDealPaginationDtoAsync ───────────────────────────────

    [Test]
    public void GetSectionDealsDealPaginationDtoAsync_WhenSectionDealNotFound_ThrowsMissingSectionPageException()
    {
        SetupSectionDealsPages(Array.Empty<SectionDeals>());

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetSectionDealsDealPaginationDtoAsync(
                "de-de",
                Guid.NewGuid(),
                new CardsQuery(),
                CancellationToken.None));
    }

    [Test]
    public void GetSectionDealsDealPaginationDtoAsync_ExceptionMessageContainsPageGuid()
    {
        SetupSectionDealsPages(Array.Empty<SectionDeals>());
        var pageGuid = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetSectionDealsDealPaginationDtoAsync(
                "en-gb",
                pageGuid,
                new CardsQuery(),
                CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_WhenSectionDealExists_CallsDealCardsServiceGetFilteredItems()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupSectionDealsPages(new[] { page });

        var expectedPagination = new PaginationDto<DealCardDto>
        {
            Data = Array.Empty<DealCardDto>(),
            TotalCount = 0,
        };

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPagination);

        var result = await _service.GetSectionDealsDealPaginationDtoAsync(
            "fr-fr",
            page.SystemFields.ContentItemGUID,
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result, Is.SameAs(expectedPagination));
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_PassesLanguageParamToService()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupSectionDealsPages(new[] { page });

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<DealCardDto> { Data = Array.Empty<DealCardDto>(), TotalCount = 0 });

        await _service.GetSectionDealsDealPaginationDtoAsync(
            "fr-fr",
            page.SystemFields.ContentItemGUID,
            new CardsQuery(),
            CancellationToken.None);

        // The language passed as a route param must flow through to IDealCardsService
        _dealCardsServiceMock.Verify(
            x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                "fr-fr",
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_PassesCardsQueryToService()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupSectionDealsPages(new[] { page });

        var query = new CardsQuery
        {
            CurrentPage = 2,
            PageSize = 10,
            Filters = new Dictionary<string, string> { ["country"] = "US" },
        };

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<DealCardDto> { Data = Array.Empty<DealCardDto>(), TotalCount = 0 });

        await _service.GetSectionDealsDealPaginationDtoAsync(
            "fr-fr",
            page.SystemFields.ContentItemGUID,
            query,
            CancellationToken.None);

        _dealCardsServiceMock.Verify(
            x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.Is<CardsQuery>(q =>
                    q.CurrentPage == 2 &&
                    q.PageSize == 10 &&
                    q.Filters.ContainsKey("country")),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_WhenPageGuidMatchesWebPageItemGUID_ReturnsSectionDeals()
    {
        // This test covers the .Or() clause that checks both ContentItemGUID and WebPageItemGUID
        var contentItemGuid = Guid.NewGuid();
        var webPageItemGuid = Guid.NewGuid();
        var pageGuid = webPageItemGuid; // Pass the WebPageItemGUID as the pageGuid parameter

        var page = new SectionDeals
        {
            IsGlobal = false,
            SectionDealsFilteringEnabled = false,
            GlobalSectionDealsFilters = null,
            SectionDealsFilters = null,
            SystemFields = new WebPageFields
            {
                ContentItemID = 100,
                ContentItemGUID = contentItemGuid, // Different from pageGuid
                ContentItemName = "Section Deals",
                WebPageItemID = 200,
                WebPageItemGUID = webPageItemGuid, // Matches pageGuid
                WebPageItemName = "Section Deals Page",
                WebPageUrlPath = "/deals",
                WebPageItemTreePath = "/deals",
            },
        };

        SetupSectionDealsPages(new[] { page });

        var expectedPagination = new PaginationDto<DealCardDto>
        {
            Data = Array.Empty<DealCardDto>(),
            TotalCount = 0,
        };

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPagination);

        var result = await _service.GetSectionDealsDealPaginationDtoAsync(
            "en-gb",
            pageGuid, // Pass WebPageItemGUID instead of ContentItemGUID
            new CardsQuery(),
            CancellationToken.None);

        Assert.That(result, Is.SameAs(expectedPagination));
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_QueriesWithPassedLanguageAndNoFallbacks()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupSectionDealsPages(new[] { page });

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<DealCardDto> { Data = Array.Empty<DealCardDto>(), TotalCount = 0 });

        await _service.GetSectionDealsDealPaginationDtoAsync(
            "en-gb",
            page.SystemFields.ContentItemGUID,
            new CardsQuery(),
            CancellationToken.None);

        // The SectionDeals lookup must use the route language with no fallbacks
        _contentServiceMock.Verify(
            x => x.GetPagesAsync<SectionDeals>(
                It.Is<RetrievePagesParameters>(p =>
                    p.LanguageName == "en-gb" &&
                    !p.UseLanguageFallbacks),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSectionDealsDealPaginationDtoAsync_ReturnsResultFromDealCardsService()
    {
        var page = CreateDefaultSectionDealsPage();
        SetupSectionDealsPages(new[] { page });

        var expectedCards = new[]
        {
            new DealCardDto { Title = "Deal 1", Country = "US" },
            new DealCardDto { Title = "Deal 2", Country = "UK" },
        };

        var expectedPagination = new PaginationDto<DealCardDto>
        {
            Data = expectedCards,
            TotalCount = 2,
        };

        _dealCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionDeals>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<CMS.ContentEngine.ContentItemQueryBuilder>>(),
                It.IsAny<Func<Deal, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPagination);

        var result = await _service.GetSectionDealsDealPaginationDtoAsync(
            "fr-fr",
            page.SystemFields.ContentItemGUID,
            new CardsQuery(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.EqualTo(expectedCards));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
    }
}