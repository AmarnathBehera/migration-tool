using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
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

public class InsightSectionCardListingPageServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInsightCardService> _insightCardServiceMock = null!;
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
    private InsightSectionCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _insightCardServiceMock = new Mock<IInsightCardService>();
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _heroServiceMock = new Mock<IHeroService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

        _websiteChannelContextMock
            .Setup(x => x.WebsiteChannelID)
            .Returns(17);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto { Layout = "default" });

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
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

        _service = new InsightSectionCardListingPageService(
            _filteringHelperMock.Object,
            _heroServiceMock.Object,
            _contentRetrieverMock.Object,
            _insightCardServiceMock.Object,
            _websiteChannelContextMock.Object,
            _webPageUrlRetrieverMock.Object);
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsContentItemNotFoundException_WhenPageIsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SectionInsightsPage>());

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightGroup>());

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightCategory>());

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsMissingSectionPageException_WhenNoSectionInsightsPageExists()
    {
        var pageGuid = Guid.NewGuid();
        var groupPage = new InsightGroup
        {
            SystemFields = new WebPageFields { WebPageItemTreePath = "/Insights/Category" },
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>()
        };

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SectionInsightsPage>());

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { groupPage });

        var exception = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(nameof(SectionInsightsPage)));
    }

    [Test]
    public void GetContentSectionDtoAsync_ThrowsArgumentNullException_WhenPageHasUninitializedCollections()
    {
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _service.GetContentSectionDtoAsync(new SectionInsightsPage(), CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public async Task GetContentPaginationResultAsync_PassesDecorateQueryAndFeaturedPredicate()
    {
        var pageGuid = Guid.NewGuid();
        var treePath = "/Insights/Section";

        var sectionPage = new SectionInsightsPage
        {
            SystemFields = new WebPageFields { WebPageItemTreePath = treePath },
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>()
        };

        _contentRetrieverMock
            .SetupSequence(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { sectionPage })
            .ReturnsAsync(new[]
            {
                new SectionInsightsPage
                {
                    InsightListFeaturedItems = new[]
                    {
                        CreateInsight(101),
                        CreateInsight(0)
                    }
                }
            });

        Action<RetrievePagesQueryParameters>? capturedDecorateQuery = null;
        Func<Insight, bool>? capturedIsFeatured = null;

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>((_, _, decorateQuery, isFeatured, _) =>
            {
                capturedDecorateQuery = decorateQuery;
                capturedIsFeatured = isFeatured;
            })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(capturedDecorateQuery, Is.Not.Null);
            Assert.That(capturedIsFeatured, Is.Not.Null);
            Assert.That(capturedIsFeatured!(CreateInsight(101)), Is.True);
            Assert.That(capturedIsFeatured!(CreateInsight(999)), Is.False);
        });
    }

    [Test]
    public async Task GetContentSectionDtoAsync_ReturnsMappedDto_WithFilteringEnabled()
    {
        var pageGuid = Guid.NewGuid();
        var page = CreateSectionInsightsPage(pageGuid);
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
            .Setup(x => x.RetrieveCurrentPage<SectionInsightsPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _heroServiceMock
            .Setup(x => x.GetHeroDtoAsync(It.IsAny<BDO.GWT.Core.Data.HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHero);

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.GetContentSectionDtoAsync(page, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo(page.MetadataTitle));
            Assert.That(result.Hero, Is.SameAs(expectedHero));
            Assert.That(result.CardsGrid.Filters, Is.SameAs(expectedFilters));
            Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.True);
            Assert.That(result.CardsGrid.ApiEndpointName, Is.EqualTo($"Insights/{pageGuid}"));
        });
        }

    private static Insight CreateInsight(int webPageItemId)
    {
        return new Insight
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = webPageItemId
            }
        };
    }

    private static SectionInsightsPage CreateSectionInsightsPage(Guid pageGuid)
    {
        return new SectionInsightsPage
        {
            MetadataTitle = "Section Insights",
            MetadataDescription = "Description",
            MetadataCanonical = "/insights",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            InsightListFilters = "businessLine",
            ItemListFilteringEnabled = true,
            ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>(),
            SystemFields = new WebPageFields
            {
                ContentItemID = 10,
                ContentItemGUID = pageGuid,
                ContentItemName = "Section Insights",
                ContentItemContentTypeID = 5,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-2),
                ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow,
                WebPageItemGUID = pageGuid,
                WebPageItemID = 11,
                WebPageItemName = "Section Insights",
                WebPageItemWebsiteChannelId = 17,
                WebPageUrlPath = "/insights",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/Insights/Section",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 2
            }
        };
    }
}