using BDO;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCategoryCardListingPageServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInsightCardService> _insightCardServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private InsightCategoryCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _insightCardServiceMock = new Mock<IInsightCardService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _heroServiceMock = new Mock<IHeroService>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
        _contentServiceMock = new Mock<IContentService>();

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelID)
            .Returns(17);

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto
            {
                Layout = "default"
            });

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
                TotalCount = 0
            });

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("en-US");

        _service = new InsightCategoryCardListingPageService(
            _insightCardServiceMock.Object,
            _websiteChannelContextMock.Object,
            _contentRetrieverMock.Object,
            _filteringHelperMock.Object,
            _heroServiceMock.Object,
            _webPageUrlRetrieverMock.Object,
            _contentServiceMock.Object);
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsContentItemNotFoundException_WhenPageIsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<InsightCategory>());

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public void GetContentPaginationResultAsync_DoesNotThrow_WhenPageIsFound()
    {
        var pageGuid = Guid.NewGuid();
        var page = CreateInsightCategory(pageGuid, "/Insights/Category");

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        Assert.DoesNotThrowAsync(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task GetContentPaginationResultAsync_PassesTreePathAndDecorateQuery_WhenTreePathAvailable()
    {
        var pageGuid = Guid.NewGuid();
        var treePath = "/Insights/Category";

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateInsightCategory(pageGuid, treePath) });

        string? capturedParentPath = null;
        Action<RetrievePagesQueryParameters>? capturedDecorateQuery = null;

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((_, parentPath, _, decorateQuery, _, _) =>
            {
                capturedParentPath = parentPath;
                capturedDecorateQuery = decorateQuery;
            })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(capturedParentPath, Is.EqualTo(treePath));
            Assert.That(capturedDecorateQuery, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetContentPaginationResultAsync_PassesNullDecorateQuery_WhenTreePathIsWhitespace()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateInsightCategory(pageGuid, "   ") });

        Action<RetrievePagesQueryParameters>? capturedDecorateQuery = _ => { };

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((_, _, _, decorateQuery, _, _) =>
            {
                capturedDecorateQuery = decorateQuery;
            })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None);

        Assert.That(capturedDecorateQuery, Is.Null);
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsNullReferenceException_WhenPageIsNull()
    {
        var exception = Assert.ThrowsAsync<NullReferenceException>(async () =>
            await _service.GetInsightPageDtoAsync(null!, CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsInvalidOperationException_WhenSystemFieldsIsNull()
    {
        // A freshly constructed InsightCategory has a null SystemFields, which
        // exercises the second guard clause once RetrieveCurrentPage succeeds.
        var pageWithoutSystemFields = new InsightCategory();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetInsightPageDtoAsync(pageWithoutSystemFields, CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain("System fields for InsightCategory page are unavailable"));
    }

    [Test]
    public async Task GetInsightPageDtoAsync_ReturnsMappedDto_WithFilteringEnabled()
    {
        var pageGuid = Guid.NewGuid();
        var currentPage = CreateInsightCategory(pageGuid, "/Insights/Category");
        var expectedHero = new HeroDto { Layout = "HeroLayout", Title = "Hero" };
        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "businessLine",
                Options = Array.Empty<OptionDto>()
            }
        };

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SectionInsightsPage
                {
                    InsightListFilters = "businessLine; serviceArea",
                    ItemListFilteringEnabled = true,
                    ItemListAdvancedSearchDescription = "Pre-link",
                    ItemListAdvancedSearchLinkText = "Advanced search",
                    ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>()
                }
            });

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.GetInsightPageDtoAsync(currentPage, CancellationToken.None);
        Assert.Multiple(() =>
        {
        Assert.That(result.Title, Is.EqualTo(currentPage.MetadataTitle));
        Assert.That(result.Hero, Is.SameAs(expectedHero));
        Assert.That(result.CardsGrid.Filters, Is.SameAs(expectedFilters));
        Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.True);
        Assert.That(result.CardsGrid.ApiEndpointName, Is.EqualTo($"Insights/{pageGuid}"));
        Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.EqualTo("Pre-link"));
        Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.EqualTo("Advanced search"));
        });
        _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(
            It.Is<HashSet<string>>(set =>
                set.Contains("businessLine") &&
                set.Contains("serviceArea") &&
                set.Count == 2),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInsightPageDtoAsync_SetsFiltersToNull_WhenFilteringDisabled()
    {
        var pageGuid = Guid.NewGuid();
        var currentPage = CreateInsightCategory(pageGuid, "/Insights/Category");

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SectionInsightsPage
                {
                    InsightListFilters = "businessLine",
                    ItemListFilteringEnabled = false,
                    ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>()
                }
            });

        var result = await _service.GetInsightPageDtoAsync(currentPage, CancellationToken.None);

        Assert.That(result.CardsGrid.Filters, Is.Null);

        _filteringHelperMock.Verify(x => x.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInsightPageDtoAsync_UsesDefaults_WhenSectionInsightsPageNotFound()
    {
        var pageGuid = Guid.NewGuid();
        var currentPage = CreateInsightCategory(pageGuid, "/Insights/Category");

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SectionInsightsPage>());

        var result = await _service.GetInsightPageDtoAsync(currentPage, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CardsGrid.Filters, Is.Null);
            Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.Null);
            Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.Null);
            Assert.That(result.CardsGrid.AdvancedSearchLink, Is.Null);
        });

        _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(
            It.Is<HashSet<string>>(set => set.Count == 0),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
        _filteringHelperMock.Verify(x => x.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInsightPageDtoAsync_ParsesWhitespaceFiltersAsEmptySet()
    {
        var pageGuid = Guid.NewGuid();
        var currentPage = CreateInsightCategory(pageGuid, "/Insights/Category");
        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "empty",
                Options = Array.Empty<OptionDto>()
            }
        };

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SectionInsightsPage
                {
                    InsightListFilters = "   ",
                    ItemListFilteringEnabled = true,
                    ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>()
                }
            });

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.GetInsightPageDtoAsync(currentPage, CancellationToken.None);

        Assert.That(result.CardsGrid.Filters, Is.SameAs(expectedFilters));

        _insightCardServiceMock.Verify(x => x.CreateFiltersAsync(
            It.Is<HashSet<string>>(set => set.Count == 0),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static InsightCategory CreateInsightCategory(Guid pageGuid, string treePath)
    {
        return new InsightCategory
        {
            MetadataTitle = "Insight category",
            MetadataDescription = "Category description",
            MetadataCanonical = "/insights/category",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            SystemFields = new WebPageFields
            {
                ContentItemID = 10,
                ContentItemGUID = pageGuid,
                ContentItemName = "Insight category",
                ContentItemContentTypeID = 5,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-2),
                ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow,
                WebPageItemGUID = pageGuid,
                WebPageItemID = 11,
                WebPageItemName = "Insight category",
                WebPageItemWebsiteChannelId = 17,
                WebPageUrlPath = "/insights/category",
                WebPageItemOrder = 1,
                WebPageItemTreePath = treePath,
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 2
            }
        };
    }
}