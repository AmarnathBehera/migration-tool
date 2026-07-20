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

public interface IInsightCategoryCardListingPageService
{
    Task<InsightCardListingDto> GetInsightPageDtoAsync(InsightCategory insightPage, CancellationToken cancellationToken);

    Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class InsightCategoryCardListingPageService : IInsightCategoryCardListingPageService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInsightCardService _insightCardService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IContentService _contentService;

    public InsightCategoryCardListingPageService(IInsightCardService insightCardService, IWebsiteChannelContext websiteChannelContext, IContentRetriever contentRetriever, IFilteringHelper filteringHelper, IHeroService heroService, IWebPageUrlRetriever webPageUrlRetriever, IContentService contentService)
    {
        _filteringHelper = filteringHelper;
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _insightCardService = insightCardService;
        _webPageUrlRetriever = webPageUrlRetriever;
        _contentService = contentService;
    }

    public async Task<InsightCardListingDto> GetInsightPageDtoAsync(InsightCategory insightPage, CancellationToken cancellationToken)
    {
        var sys = insightPage.SystemFields;
        if (sys is null)
        {
            throw new InvalidOperationException("System fields for InsightCategory page are unavailable.");
        }

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
        var page = (await _contentRetriever.RetrievePages<InsightCategory, InsightCategory>(
            new RetrievePagesParameters(),
            query => query.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemGUID), new[] { pageGuid })),
            RetrievalCacheSettings.CacheDisabled,
            (_, item) => Task.FromResult(item),
            cancellationToken)).FirstOrDefault();

        if (page is null)
        {
            throw new ContentItemNotFoundException($"InsightCategory page not found for guid '{pageGuid}'.");
        }

        var treePath = page.SystemFields?.WebPageItemTreePath ?? string.Empty;
        Action<RetrievePagesQueryParameters>? decorateQuery = string.IsNullOrWhiteSpace(treePath)
            ? null
            : q => q.Where(w => w.WhereStartsWith(nameof(WebPageFields.WebPageItemTreePath), $"{treePath}/"));

        var featuredContentPageIds = new HashSet<int>();

        return await _insightCardService.GetFilteredItemsAsync(
            cardsQuery,
            treePath,
           _contentService.GetCurrentLanguage(),
            decorateQuery,
            contentPage => featuredContentPageIds.Contains(contentPage.SystemFields.WebPageItemID),
            cancellationToken);
    }


    private async Task<CardsGridDto> GetCardsGridDtoAsync(InsightCategory page, CancellationToken cancellationToken)
    {
        var insightSection = await SectionInsightsPageRetriever.GetSectionInsightsPageAsync(
            _contentRetriever,
            _websiteChannelContext,
            cancellationToken);

        return await InsightCardsGridFactory.CreateAsync(
            page.SystemFields.WebPageItemGUID,
            insightSection,
            _insightCardService,
            _filteringHelper,
            _webPageUrlRetriever,
            cancellationToken: cancellationToken);
    }
}
