using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface INewsArticleCardsService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken);
    Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync
        (CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<NewsArticle, bool> isFeatured,
        CancellationToken cancellationToken);
}

public class NewsArticleCardsService : INewsArticleCardsService
{
    private readonly ICardsService _cardsService;
    //private readonly IChildPageHelper _childPageHelper;
    //private readonly IContentPageCardHelper<NewsArticle> _newsArticleCardHelper;
    private readonly ICultureContext _cultureContext;
    private readonly IContentService _contentService;

    public NewsArticleCardsService(ICardsService cardsService,
       // IChildPageHelper childPageHelper,
        ICultureContext cultureContext, IContentService contentService )
    {
        _cardsService = cardsService;
        //_childPageHelper = childPageHelper;
        _cultureContext = cultureContext;
        _contentService = contentService;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
    }

    public async Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync
        (CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery, 
        Func<NewsArticle, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

        var newsArticles = await _contentService.RetrievePagesAsync<NewsArticle>(
              new RetrievePagesParameters
              {
                  LinkedItemsMaxLevel=2,
                  LanguageName = _cultureContext.CurrentLanguageTag,
                  UseLanguageFallbacks = false
              },
           query =>
           {
               query.Where(w =>
               {
                   w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                   foreach (var filter in filterWhereExpressions)
                   {
                       filter(w);
                   }
               });
           },
           cancellationToken
       );
       
        var orderedNewsArticles = newsArticles
            .OrderByDescending(isFeatured)
            //.ThenByDescending(x => x.PublishFrom ?? x.CreationDate)
            .ToArray();

        //var paginatedNewsArticles = PaginationHelper.PaginatePages(orderedNewsArticles, cardsQuery)
        //    .ToArray();

        var itemDtos = await GetContentPageCardListAsync(orderedNewsArticles, _cultureContext.CurrentLanguageTag, isFeatured, cancellationToken);

        var paginationDto = new PaginationDto<ContentPageCardDto>
        {
            Data = itemDtos.ToArray(),
            TotalCount = orderedNewsArticles.Length,
        };

        return paginationDto;
    }

    //private static void ApplyCommonFilter(IPageQuery<NewsArticle> pageQuery)
    //{
    //    pageQuery.Where(x => x.IsSearchExcluded == false || x.IsSearchExcluded == null);
    //}

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
            //FilterParameterName.BusinessLine => await BuildBusinessLineWhereExpression(page.NodeAliasPath),
            //FilterParameterName.ServiceArea => await BuildServiceAreaWhereExpression(page.NodeAliasPath),
            FilterParameterName.Service => x => x.WhereContains(nameof(NewsArticle.MetadataServices), guid),
            //FilterParameterName.IndustryCategory => await BuildIndustryCategoryWhereExpression(page.NodeAliasPath),
            FilterParameterName.IndustryPage => x => x.WhereContains(nameof(NewsArticle.MetadataIndustries), guid),
            //FilterParameterName.MicrositeMasterPage => await BuildMicrositeMasterPageWhereExpression(page.NodeAliasPath),
            _ => null,
        };
    }

    //private async Task<Expression<Func<NewsArticle, bool>>?> BuildBusinessLineWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(BusinessLine),
    //        typeof(ServiceArea),
    //        typeof(Service),
    //    ], nodeAliasPathPrefix);

    //    var businessLineNodeIds = PageTypeUtility.GetPageTypeNodeIds<BusinessLine>(nodeIdsByPageType);
    //    var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
    //    var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataBusinessLines.Intersect(businessLineNodeIds).Any() ||
    //        x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
    //        x.MetadataServices.Intersect(serviceNodeIds).Any();
    //}

    //private async Task<Expression<Func<NewsArticle, bool>>> BuildServiceAreaWhereExpression(string pageNodeAliasPath)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(ServiceArea),
    //        typeof(Service),
    //    ], pageNodeAliasPath);

    //    var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
    //    var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
    //        x.MetadataServices.Intersect(serviceNodeIds).Any();
    //}

    //private async Task<Expression<Func<NewsArticle, bool>>?> BuildIndustryCategoryWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(IndustryCategory),
    //        typeof(Industry),
    //    ], nodeAliasPathPrefix);

    //    var industryCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryCategory>(nodeIdsByPageType);
    //    var industryNodeIds = PageTypeUtility.GetPageTypeNodeIds<Industry>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataIndustryCategories.Intersect(industryCategoryNodeIds).Any() ||
    //        x.MetadataIndustries.Intersect(industryNodeIds).Any();
    //}

    //private async Task<Expression<Func<NewsArticle, bool>>?> BuildMicrositeMasterPageWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(MicrositeMasterPage),
    //        typeof(MicrositePage),
    //    ], nodeAliasPathPrefix);

    //    var micrositeMasterPageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositeMasterPage>(nodeIdsByPageType);
    //    var micrositePageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositePage>(nodeIdsByPageType);

    //    return x =>
    //        x.MicrositeMasterPages.Intersect(micrositeMasterPageNodeIds).Any() ||
    //        x.MicrositePages.Intersect(micrositePageNodeIds).Any();
    //}

    public async Task<List<ContentPageCardDto>> GetContentPageCardListAsync(ICollection<NewsArticle> contentPages, string languageTag, Func<NewsArticle, bool> isPageFeatured, CancellationToken cancellationToken)
    {
        var cards = new List<ContentPageCardDto>();
        foreach (var contentPage in contentPages)
        {
            bool isComingSoon = false;

            //if (contentPage is Insight insightPage)
            //{
            //    isComingSoon = insightPage.ComingSoon;
            //}
            var card = new ContentPageCardDto
            {
                IsFeatured = isPageFeatured(contentPage),
                ImageURL = contentPage.ImageFile.FirstOrDefault()?.ContentAttachmentAsset.Url,
                isComingSoon = isComingSoon,
                ImageAltText = contentPage.ImageFileAltText,
                //Type = await contentPage.GetTypeNameAsync(_contentTypeRetriever, cancellationToken),
                Title = contentPage.MetadataTitle,
               // Date = contentPage.PublishFrom ?? contentPage.CreationDate,
                UpdatedOnDate = contentPage.UpdatedOn == DateTime.MinValue ? null : contentPage.UpdatedOn,
                Description = HtmlHelper.ExtractTextFromHtml(contentPage.MetadataDescription),
                ReadMoreLink = new LinkItemDto
                {
                    Title = ResourceStringKey.LiveSite.ReadMore,
                    URL = contentPage.GetUrl().AbsoluteUrl
                },
            };
            cards.Add(card);
        }
        return cards;
    }
}
