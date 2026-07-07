using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class DealCardsServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private DealCardsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _httpContextAccessorMock
            .Setup(a => a.HttpContext)
            .Returns(new DefaultHttpContext());

        _service = new DealCardsService(
            _cardsServiceMock.Object,
            _contentServiceMock.Object,
            _httpContextAccessorMock.Object);
    }

    private static SectionDeals CreateSectionDeals(int webPageItemId = 100) => new()
    {
        SystemFields = new WebPageFields
        {
            WebPageItemID = webPageItemId,
            WebPageItemGUID = Guid.NewGuid(),
            WebPageItemName = "section-deals",
            WebPageUrlPath = "/section-deals"
        }
    };

    private void SetupDealsReturns(IEnumerable<Deal>? deals)
    {
        _contentServiceMock
            .Setup(x => x.GetPagesAsync<Deal>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deals!);
    }

    private static IWebPageFieldsSource CreateMockPage(Guid guid)
    {
        var mock = new Mock<IWebPageFieldsSource>();
        mock.Setup(p => p.SystemFields).Returns(new WebPageFields { ContentItemGUID = guid });
        return mock.Object;
    }

    // ── CreateFiltersAsync ──────────────────────────────────────────────────

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var expected = new FilterItemDto<OptionDto>[] { new() { Title = "Industry" } };
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(
            new HashSet<string> { FilterParameterName.Industry }, "/deals", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Industry"));
        });
        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), "/deals", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsEmptyList_WhenCardsServiceReturnsEmpty()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var result = await _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CreateFiltersAsync_PassesNodeAliasPathToCardsService()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.CreateFiltersAsync(new HashSet<string>(), "/path/to/deals", CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), "/path/to/deals", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetFilteredItemsAsync – core orchestration ──────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WhenContentServiceReturnsNull_ReturnsEmptyPagination()
    {
        SetupDealsReturns(null);

        var result = await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WhenContentServiceReturnsEmpty_ReturnsZeroTotalCount()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());

        var result = await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithNoFilters_DoesNotCallGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", new CardsQuery(), null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetFilteredItemsAsync_AlwaysCallsGetPagesAsyncForDeals()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(200), "en-us", new CardsQuery(), null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPagesAsync<Deal>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetFilteredItemsAsync – Year filter ─────────────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WithValidYearFilter_DoesNotCallGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.Year] = "2024" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithInvalidYearFilter_DoesNotCallGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.Year] = "not-a-year" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── GetFilteredItemsAsync – Country filter ──────────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WithCountryFilter_DoesNotCallGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.Country] = "Belgium" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── GetFilteredItemsAsync – page-based filters ──────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WithIndustryFilter_CallsGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        _contentServiceMock
            .Setup(x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardIndustry), "Finance", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockPage(Guid.NewGuid()));

        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.Industry] = "Finance" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardIndustry), "Finance", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithIndustryFilter_WhenPageNotFound_SkipsFilter()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        _contentServiceMock
            .Setup(x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardIndustry), "Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.Industry] = "Unknown" }
        };

        var result = await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithSubIndustryFilter_CallsGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        _contentServiceMock
            .Setup(x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardSubIndustry), "Banking", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockPage(Guid.NewGuid()));

        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.SubIndustry] = "Banking" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardSubIndustry), "Banking", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithDealTypeFilter_CallsGetPageByTypeAndName()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        _contentServiceMock
            .Setup(x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardDealType), "M&A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockPage(Guid.NewGuid()));

        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { [FilterParameterName.DealType] = "M&A" }
        };

        await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                typeof(DealCardDealType), "M&A", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetFilteredItemsAsync – unknown filter ──────────────────────────────

    [Test]
    public async Task GetFilteredItemsAsync_WithUnknownFilter_IsIgnoredAndReturnsEmptyData()
    {
        SetupDealsReturns(Enumerable.Empty<Deal>());
        var query = new CardsQuery
        {
            Filters = new Dictionary<string, string> { ["unknownFilter"] = "someValue" }
        };

        var result = await _service.GetFilteredItemsAsync(
            CreateSectionDeals(), "en", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
        _contentServiceMock.Verify(
            x => x.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
