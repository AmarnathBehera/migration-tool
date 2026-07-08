using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;
public interface IInsightSectionCardListingPageService
{
    Task<InsightCardListingDto> GetContentSectionDtoAsync(SectionInsightsPage insightPage, CancellationToken cancellationToken);

    Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class InsightSectionCardListingPageService : IInsightSectionCardListingPageService

{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInsightCardService _insightCardService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public InsightSectionCardListingPageService(
        IFilteringHelper filteringHelper,
        IHeroService heroService,
        IContentRetriever contentRetriever,
        IInsightCardService insightCardService,
        IWebsiteChannelContext websiteChannelContext,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _filteringHelper = filteringHelper;
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _insightCardService = insightCardService;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public async Task<InsightCardListingDto> GetContentSectionDtoAsync(SectionInsightsPage insightPage, CancellationToken cancellationToken)
    {
        return await InsightListingDtoMapper.MapToInsightCardListingDtoAsync(
            insightPage,
            _heroService,
            async page => await GetCardsGridDtoAsync(page, cancellationToken),
            cancellationToken);
    }

    public async Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(
        Guid pageGuid,
        CardsQuery cardsQuery,
        CancellationToken cancellationToken)
    {
        var treePath = await GetTreePathAsync<SectionInsightsPage>(pageGuid, cancellationToken)
            ?? await GetTreePathAsync<InsightGroup>(pageGuid, cancellationToken)
            ?? await GetTreePathAsync<InsightCategory>(pageGuid, cancellationToken);

        if (string.IsNullOrWhiteSpace(treePath))
        {
            throw new ContentItemNotFoundException($"SectionInsightsPage page not found for guid '{pageGuid}'.");
        }

        Action<RetrievePagesQueryParameters>? decorateQuery = string.IsNullOrWhiteSpace(treePath)
            ? null
            : q => q.Where(w => w.WhereStartsWith(nameof(WebPageFields.WebPageItemTreePath), $"{treePath}/"));

        var featuredContentPageIds = await GetFeaturedContentPageIdsAsync(cancellationToken);

        return await _insightCardService.GetFilteredItemsAsync(
            cardsQuery,
            treePath,
            decorateQuery,
            contentPage => featuredContentPageIds.Contains(contentPage.SystemFields.WebPageItemID),
            cancellationToken);
    }

    private async Task<string?> GetTreePathAsync<TPage>(Guid pageGuid, CancellationToken cancellationToken)
        where TPage : class, IWebPageFieldsSource
    {
        var page = (await _contentRetriever.RetrievePages<TPage, TPage>(
            new RetrievePagesParameters(),
            query => query.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemGUID), new[] { pageGuid })),
            RetrievalCacheSettings.CacheDisabled,
            (_, item) => Task.FromResult(item),
            cancellationToken))
            .FirstOrDefault();

        return page?.SystemFields?.WebPageItemTreePath;
    }

    private async Task<SectionInsightsPage> GetInsightSectionPage(SectionInsightsPage? currentPage, CancellationToken cancellationToken)
    {
        if (currentPage is not null)
        {
            return currentPage;
        }

        return await SectionInsightsPageRetriever.GetRequiredSectionInsightsPageAsync(
            _contentRetriever,
            _websiteChannelContext,
            cancellationToken);
    }

    private async Task<ISet<int>> GetFeaturedContentPageIdsAsync(CancellationToken cancellationToken)
    {
        var insightSection = await GetInsightSectionPage(null, cancellationToken);

        return insightSection.InsightListFeaturedItems?
            .Select(x => x.SystemFields.WebPageItemID)
            .Where(id => id > 0)
            .ToHashSet()
            ?? new HashSet<int>();
    }

    private async Task<CardsGridDto> GetCardsGridDtoAsync(SectionInsightsPage page, CancellationToken cancellationToken)
    {
        var insightSection = await GetInsightSectionPage(page, cancellationToken);

        return await InsightCardsGridFactory.CreateAsync(
            page.SystemFields.WebPageItemGUID,
            insightSection,
            _insightCardService,
            _filteringHelper,
            cancellationToken,
            _webPageUrlRetriever,
            includeTitleAndDescription: true);
    }
}
