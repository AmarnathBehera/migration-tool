using System.Reflection;
using System.Runtime.Serialization;
using BDO;
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

public class InsightGroupCardListingPageServiceTests
{
    private Mock<IInsightCardService> _insightCardServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;

    private InsightGroupCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _insightCardServiceMock = new Mock<IInsightCardService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _heroServiceMock = new Mock<IHeroService>();

        _service = new InsightGroupCardListingPageService(
            _insightCardServiceMock.Object,
            _websiteChannelContextMock.Object,
            _contentRetrieverMock.Object,
            _filteringHelperMock.Object,
            _heroServiceMock.Object);
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsContentItemNotFound_WhenInsightGroupPageNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightGroup>());

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentPaginationResultAsync(
                pageGuid,
                CreateCardsQuery(),
                CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsInvalidOperation_WhenTreePathIsEmpty()
    {
        var pageGuid = Guid.NewGuid();

        var page = CreateInsightGroup(
            pageGuid,
            treePath: string.Empty);

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetContentPaginationResultAsync(
                pageGuid,
                CreateCardsQuery(),
                CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("WebPageItemTreePath"));
    }

    [Test]
    public async Task GetContentPaginationResultAsync_DelegatesToInsightCardService_WithParentTreePath()
    {
        var pageGuid = Guid.NewGuid();
        var treePath = "/Insights/Group";

        var page = CreateInsightGroup(
            pageGuid,
            treePath);

        var cardsQuery = CreateCardsQuery();

        var expected = new PaginationDto<ContentPageCardDto>
        {
            Data = Array.Empty<ContentPageCardDto>(),
            TotalCount = 0
        };

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                cardsQuery,
                treePath,
                null,
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.GetContentPaginationResultAsync(
            pageGuid,
            cardsQuery,
            CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));

        _insightCardServiceMock.Verify(x => x.GetFilteredItemsAsync(
            cardsQuery,
            treePath,
            null,
            It.IsAny<Func<Insight, bool>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetContentPaginationResultAsync_PassesNullDecorateQuery_BecauseInsightCardServiceHandlesChildrenPath()
    {
        var pageGuid = Guid.NewGuid();
        var treePath = "/Insights/Group";

        var page = CreateInsightGroup(
            pageGuid,
            treePath);

        Action<RetrievePagesQueryParameters>? capturedDecorateQuery = _ => { };

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                treePath,
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>(
                (_, _, decorateQuery, _, _) =>
                {
                    capturedDecorateQuery = decorateQuery;
                })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        await _service.GetContentPaginationResultAsync(
            pageGuid,
            CreateCardsQuery(),
            CancellationToken.None);

        Assert.That(capturedDecorateQuery, Is.Null);
    }

    [Test]
    public async Task GetContentPaginationResultAsync_PassesFeaturedPredicate()
    {
        var pageGuid = Guid.NewGuid();
        var treePath = "/Insights/Group";

        var page = CreateInsightGroup(
            pageGuid,
            treePath);

        Func<Insight, bool>? capturedIsFeatured = null;

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightGroup, InsightGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                treePath,
                null,
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<CardsQuery, string, Action<RetrievePagesQueryParameters>?, Func<Insight, bool>, CancellationToken>(
                (_, _, _, isFeatured, _) =>
                {
                    capturedIsFeatured = isFeatured;
                })
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        await _service.GetContentPaginationResultAsync(
            pageGuid,
            CreateCardsQuery(),
            CancellationToken.None);

        Assert.That(capturedIsFeatured, Is.Not.Null);

        var insight = CreateInsight(webPageItemId: 123);

        Assert.That(capturedIsFeatured!(insight), Is.False);
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsInvalidOperation_WhenSystemFieldsAreNull()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetInsightPageDtoAsync(
                new InsightGroup(),
                CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("System fields"));
    }

    [Test]
    public async Task GetInsightPageDtoAsync_ReturnsMappedDto_WithFilteringEnabled()
    {
        var pageGuid = Guid.NewGuid();
        var page = CreateInsightGroup(pageGuid, "/Insights/Group");
        var expectedHero = new HeroDto { Layout = "layout", Title = "Hero" };
        var expectedFilters = new[]
        {
            new FilterItemDto<OptionDto>
            {
                Name = "businessLine",
                Options = Array.Empty<OptionDto>()
            }
        };

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

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

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _insightCardServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _service.GetInsightPageDtoAsync(page, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo(page.MetadataTitle));
            Assert.That(result.Hero, Is.SameAs(expectedHero));
            Assert.That(result.CardsGrid.Filters, Is.SameAs(expectedFilters));
            Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.True);
            Assert.That(result.CardsGrid.ApiEndpointName, Is.EqualTo($"Insights/{pageGuid}"));
            Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.EqualTo("Pre-link"));
            Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.EqualTo("Advanced search"));
        });
    }

    [Test]
    public async Task GetInsightPageDtoAsync_SetsFiltersNull_WhenFilteringDisabled()
    {
        var pageGuid = Guid.NewGuid();
        var page = CreateInsightGroup(pageGuid, "/Insights/Group");

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightGroup>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightGroup, Task<InsightGroup>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

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

        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.GetInsightPageDtoAsync(page, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.CardsGrid.Filters, Is.Null);
            Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.False);
        });
    }

    private static CardsQuery CreateCardsQuery()
    {
        return new CardsQuery
        {
            CurrentPage = 1,
            PageSize = 10,
            Filters = new Dictionary<string, string>()
        };
    }

    private static InsightGroup CreateInsightGroup(
        Guid pageGuid,
        string? treePath)
    {
        var page = new InsightGroup
        {
            MetadataTitle = "Insight Group",
            MetadataDescription = "Group description",
            MetadataCanonical = "/insights/group",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataOGImage = Array.Empty<ContentAttachment>(),
            MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>()
        };

        var systemFieldsProperty = typeof(InsightGroup).GetProperty(nameof(InsightGroup.SystemFields));
        if (systemFieldsProperty is null)
        {
            throw new InvalidOperationException("SystemFields property was not found on InsightGroup.");
        }

        var systemFields = CreateObject(systemFieldsProperty.PropertyType);

        SetPropertyValue(systemFields, "ContentItemID", 1);
        SetPropertyValue(systemFields, "ContentItemGUID", pageGuid);
        SetPropertyValue(systemFields, "ContentItemName", "Insight Group");
        SetPropertyValue(systemFields, "ContentItemContentTypeID", 1);
        SetPropertyValue(systemFields, "ContentItemIsSecured", false);
        SetPropertyValue(systemFields, "ContentItemCommonDataContentLanguageID", 1);
        SetPropertyValue(systemFields, "ContentItemCommonDataFirstPublishedWhen", DateTime.UtcNow.AddDays(-1));
        SetPropertyValue(systemFields, "ContentItemCommonDataLastPublishedWhen", DateTime.UtcNow);

        SetPropertyValue(systemFields, "WebPageItemGUID", pageGuid);
        SetPropertyValue(systemFields, "WebPageItemID", 10);
        SetPropertyValue(systemFields, "WebPageItemName", "Insight Group");
        SetPropertyValue(systemFields, "WebPageItemWebsiteChannelId", 1);
        SetPropertyValue(systemFields, "WebPageUrlPath", "/insights/group");
        SetPropertyValue(systemFields, "WebPageItemOrder", 1);
        SetPropertyValue(systemFields, "WebPageItemTreePath", treePath);
        SetPropertyValue(systemFields, "WebPageUrlPathContentLanguageID", 1);
        SetPropertyValue(systemFields, "WebPageItemParentID", 5);

        SetPropertyValue(page, nameof(InsightGroup.SystemFields), systemFields);

        return page;
    }

    private static Insight CreateInsight(int webPageItemId)
    {
        var insight = new Insight();

        var systemFieldsProperty = typeof(Insight).GetProperty(nameof(Insight.SystemFields));
        if (systemFieldsProperty is null)
        {
            throw new InvalidOperationException("SystemFields property was not found on Insight.");
        }

        var systemFields = CreateObject(systemFieldsProperty.PropertyType);

        SetPropertyValue(systemFields, "WebPageItemID", webPageItemId);

        SetPropertyValue(insight, nameof(Insight.SystemFields), systemFields);

        return insight;
    }

    private static object CreateObject(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);

        if (constructor is not null)
        {
            return Activator.CreateInstance(type)!;
        }

#pragma warning disable SYSLIB0050
        return FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
    }

    private static void SetPropertyValue(
        object target,
        string propertyName,
        object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var backingField = target.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (backingField is not null)
        {
            backingField.SetValue(target, value);
        }
    }
}