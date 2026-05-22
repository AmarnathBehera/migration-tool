using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using HotChocolate.Data.Filters;
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
    private readonly ICultureContext _cultureContext;
    private readonly IContentService _contentService;
    private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;

    public NewsArticleCardsService(
        ICardsService cardsService,
        ICultureContext cultureContext,
        IContentService contentService,
        ITaxonomyTagRetriever taxonomyTagRetriever)
    {
        _cardsService = cardsService;
        _cultureContext = cultureContext;
        _contentService = contentService;
        _taxonomyTagRetriever = taxonomyTagRetriever;
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
            .ToArray();

        var paginatedPages = orderedNewsArticles
            .Skip((cardsQuery.CurrentPage.Value - 1) * cardsQuery.PageSize.Value)
            .Take(cardsQuery.PageSize.Value)
            .ToArray();

        var itemDtos = await GetContentPageCardListAsync(paginatedPages, _cultureContext.CurrentLanguageTag, isFeatured, cancellationToken);

        var paginationDto = new PaginationDto<ContentPageCardDto>
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
                FilterParameterType.Taxonomy => await GetTaxonomyFilterAsync(filterParameterName, filterParameterValue),
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

    private async Task<Action<WhereParameters>?> GetTaxonomyFilterAsync(string filterName, string filterValue)
    {
        var tag = await _taxonomyTagRetriever.GetTaxonomyTagByNameAsync(filterValue);
        if (tag == null)
            return null;

        var identifiers = new List<Guid> { tag.TagGUID };

        return filterName switch
        {
            FilterParameterName.ContentType => w => w.WhereContainsTags(nameof(NewsArticle.ContentType), identifiers),
            _ => null,
        };
    }
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
