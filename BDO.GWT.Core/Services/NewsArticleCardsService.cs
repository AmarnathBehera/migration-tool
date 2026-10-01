using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace BDO.GWT.Core.Services;

public interface INewsArticleCardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken);
    Task<PaginationDto<NewsPageCardDto>> GetFilteredItemsAsync
        (List<SectionNews> newsSections,
        string language,
        CardsQuery cardsQuery,
        List<Action<WhereParameters>>? queryFilters,
        Func<NewsArticle, bool> isFeatured,
        CancellationToken cancellationToken);
}

public class NewsArticleCardsService : INewsArticleCardsService
{
    private readonly ICardsService _cardsService;
    private readonly ICultureContext _cultureContext;
    private readonly IContentService _contentService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebPageUrlResolver _webPageUrlResolver;

    public NewsArticleCardsService(
        ICardsService cardsService,
        ICultureContext cultureContext,
        IContentService contentService,
        IUrlHelperFactory urlHelperFactory,
        IHttpContextAccessor httpContextAccessor,
        IWebPageUrlResolver webPageUrlResolver)
    {
        _cardsService = cardsService;
        _cultureContext = cultureContext;
        _contentService = contentService;
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
        _webPageUrlResolver = webPageUrlResolver;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
    }
    public async Task<PaginationDto<NewsPageCardDto>> GetFilteredItemsAsync(
    List<SectionNews> newsSections,
    string language,
    CardsQuery cardsQuery,
    List<Action<WhereParameters>>? queryFilters,
    Func<NewsArticle, bool> isFeatured,
    CancellationToken cancellationToken)
    {
        int channelId = await _contentService.GetCurrentChannelIDChannelInfo(cancellationToken);
        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

        var newsGroupIDsList = new List<int>();

        foreach (var newsSection in newsSections)
        { 
            var sectionNewsGroupIDs = (await _contentService.GetPagesAsync<NewsGroup>(
               new RetrievePagesParameters
               {
                   LanguageName = language,
                   UseLanguageFallbacks = false
               },
               query => query
                   .Where(w => w.WhereEquals("WebPageItemParentID", newsSection.SystemFields.WebPageItemID)),
               RetrievalCacheSettings.CacheDisabled,
               cancellationToken
           ))?.Select(x => x.SystemFields.WebPageItemID).ToArray() ?? [];
           
           newsGroupIDsList.AddRange(sectionNewsGroupIDs);
        }

        var newsGroupIDs = newsGroupIDsList.ToArray();

        if (newsGroupIDs.Length == 0)
            return new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 };

        var newsArticles = await _contentService.GetPagesAsync<NewsArticle>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 4,
                LanguageName = language,
                UseLanguageFallbacks = false
            },
            query =>
            {
                query.Where(w =>
                {
                    w.WhereEquals("ContentItemChannelID", channelId);
                    w.WhereIn("WebPageItemParentID", newsGroupIDs);

                    foreach (var filter in filterWhereExpressions)
                    {
                        filter(w);
                    }
                    if (queryFilters!=null)
                    {
                        foreach (var filter in queryFilters)
                        {
                            filter(w);
                        }
                    }
                });
            },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ) ?? [];      

        var orderedNewsArticles = newsArticles
            .Where(x => x is not null)
            .OrderByDescending(isFeatured)
            .ToArray();       

        var currentPage = cardsQuery.CurrentPage ?? 1;
        var pageSize = cardsQuery.PageSize ?? 10;

        var paginatedPages = orderedNewsArticles
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var itemDtos = await GetContentPageCardListAsync(paginatedPages, _cultureContext.CurrentLanguageTag, isFeatured, cancellationToken);

        var paginationDto = new PaginationDto<NewsPageCardDto>
        {
            Data = itemDtos.ToArray(),
            TotalCount = paginatedPages.Length,
        };

        return paginationDto;
    }
    private async Task<List<Action<WhereParameters>>> GetFilterParameterWhereExpressionsAsync(IDictionary<string, string> filterParameterValueByName)
    {
        var whereExpressions = new List<Action<WhereParameters>>();

        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
        {
            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

            if (filterParameterData is null)
            {
                continue;
            }

            var filter = filterParameterData.FilterParameterType switch
            {
                FilterParameterType.Page => await GetPageFilterAsync(filterParameterName, filterParameterValue),
                FilterParameterType.Tagpage => await GetTagPageFilterAsync(filterParameterName, filterParameterValue),
                _ => null,
            };

            if (filter is not null)
            {
                whereExpressions.Add(filter);
            }
        }

        return whereExpressions;
    }

    private async Task<Action<WhereParameters>?> GetPageFilterAsync(string filterName, string filterValue)
    {
        var page = await _contentService.GetPageByTypeAndNameAsync(
            FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

        if (page is null)
            return null;

        var guid = page.SystemFields.ContentItemGUID.ToString();

        return filterName switch
        {
            FilterParameterName.BusinessLine => x => x.WhereContains(nameof(NewsArticle.MetadataBusinessLine), guid),
            FilterParameterName.ServiceArea => x => x.WhereContains(nameof(NewsArticle.MetadataServiceAreas), guid),
            FilterParameterName.Service => x => x.WhereContains(nameof(NewsArticle.MetadataServices), guid),
            FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(NewsArticle.MetadataIndustryCategories), guid),
            FilterParameterName.IndustryPage => x => x.WhereContains(nameof(NewsArticle.MetadataIndustries), guid),
            FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(NewsArticle.MicrositeMasterPages), guid),
            FilterParameterName.MicrositePage => x => x.WhereContains(nameof(NewsArticle.MicrositePages), guid),
            
            _ => null,
        };
    }

    private async Task<Action<WhereParameters>?> GetTagPageFilterAsync(string filterName, string filterValue)
    {

        var page = await _contentService.GetPageByTypeAndNameAsync(
            FilterUtility.GetFilterParameterData(filterName)!.EntityType, filterValue);

        if (page is null)
            return null;

        var guid = page.SystemFields.ContentItemGUID.ToString();

        return filterName switch
        {

            FilterParameterName.ContentType => w => w.WhereContains(nameof(NewsArticle.ContentTypeFilter), guid),

            _ => null,
        };
    }
    public async Task<List<NewsPageCardDto>> GetContentPageCardListAsync(ICollection<NewsArticle> contentPages, string languageTag, Func<NewsArticle, bool> isPageFeatured, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext!;
        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());
        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

        var cards = new List<NewsPageCardDto>();
        foreach (var contentPage in contentPages)
        {
            bool isComingSoon = false;
            var card = new NewsPageCardDto
            {
                IsFeatured = isPageFeatured(contentPage),
                ImageURL = urlHelper.Content(contentPage.ImageFileM?.FirstOrDefault()?.ContentAttachmentAsset.Url),
                isComingSoon = isComingSoon,
                ImageAltText = contentPage.ImageFileAltTextM,
                Type = contentPage.ContentTypeFilter?.FirstOrDefault()?.Name,
                Title = contentPage.MetadataTitle,
                Date = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen ?? contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                UpdatedOnDate = contentPage.UpdatedOn == DateTime.MinValue ? null : contentPage.UpdatedOn,
                Description = HtmlHelper.ExtractTextFromHtml(contentPage.MetadataDescription),
                ReadMoreLink = new LinkItemDto
                {
                    Title = ResourceStringKey.LiveSite.ReadMore,
                    URL = _webPageUrlResolver.GetAbsoluteUrl(contentPage)
                },
            };
            cards.Add(card);
        }
        return cards;
    }
}
