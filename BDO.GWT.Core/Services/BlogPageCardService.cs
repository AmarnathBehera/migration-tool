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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace BDO.GWT.Core.Services
{
    public interface IBlogPageCardsService
    {
        Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken);
        Task<PaginationDto<BlogPageCardDto>> GetFilteredItemsAsync
            (string language,
            CardsQuery cardsQuery,
            Action<ContentItemQueryBuilder>? decorateQuery,
            Func<BlogPost, bool> isFeatured,
            CancellationToken cancellationToken);
    }
    public class BlogPageCardsService : IBlogPageCardsService
    {
        private readonly ICardsService _cardsService;
        private readonly ICultureContext _cultureContext;
        private readonly IContentService _contentService;
        private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogPageCardsService(
            ICardsService cardsService,
            ICultureContext cultureContext,
            IContentService contentService,
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor,
            ITaxonomyTagRetriever taxonomyTagRetriever)
        {
            _cardsService = cardsService;
            _cultureContext = cultureContext;
            _contentService = contentService;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _taxonomyTagRetriever = taxonomyTagRetriever;
        }

        public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
        {
            return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
        }

        public async Task<PaginationDto<BlogPageCardDto>> GetFilteredItemsAsync
            (string language,
            CardsQuery cardsQuery,
            Action<ContentItemQueryBuilder>? decorateQuery,
            Func<BlogPost, bool> isFeatured,
            CancellationToken cancellationToken)
        {
            var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

            var blogPosts = await _contentService.GetPagesAsync<BlogPost>(
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
                       w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                       foreach (var filter in filterWhereExpressions)
                       {
                           filter(w);
                       }
                   });
               },
                    RetrievalCacheSettings.CacheDisabled,
               cancellationToken
           ) ?? [];

            var orderedBlogPosts = blogPosts
                .Where(x => x is not null)
                .Cast<BlogPost>()
                .OrderByDescending(isFeatured)
                .ToArray();

            var currentPage = cardsQuery.CurrentPage ?? 1;
            var pageSize = cardsQuery.PageSize ?? 10;

            var paginatedPages = orderedBlogPosts
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            var itemDtos = await GetContentPageCardListAsync(paginatedPages, _cultureContext.CurrentLanguageTag, isFeatured, cancellationToken);

            var paginationDto = new PaginationDto<BlogPageCardDto>
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
                FilterParameterName.BusinessLine => x => x.WhereContains(nameof(BlogPost.MetadataBusinessLine), guid),
                FilterParameterName.ServiceArea => x => x.WhereContains(nameof(BlogPost.MetadataServiceAreas), guid),
                FilterParameterName.Service => x => x.WhereContains(nameof(BlogPost.MetadataServices), guid),
                FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(BlogPost.MetadataIndustryCategories), guid),
                FilterParameterName.IndustryPage => x => x.WhereContains(nameof(BlogPost.MetadataIndustries), guid),
                FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(BlogPost.MicrositeMasterPages), guid),
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
        public async Task<List<BlogPageCardDto>> GetContentPageCardListAsync(ICollection<BlogPost> contentPages,
            string languageTag, Func<BlogPost, bool> isPageFeatured, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor());
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            var cards = new List<BlogPageCardDto>();
            foreach (var contentPage in contentPages)
            {
                //var tags = await _taxonomyTagRetriever.GetTaxonomyTagByGUIDs(contentPage.ContentType, _cultureContext.CurrentLanguageTag);
                IEnumerable<Contenttypes> type = contentPage.ContentType;
                var typeConst = "Blog";
                bool isComingSoon = false;
                var card = new BlogPageCardDto
                {
                    IsFeatured = isPageFeatured(contentPage),
                    ImageURL = urlHelper.Content(contentPage.ImageFile.FirstOrDefault()?.ContentAttachmentAsset.Url),
                    isComingSoon = isComingSoon,
                    ImageAltText = contentPage.ImageFileAltText,
                    Type = typeConst,
                    Title = contentPage.MetadataTitle,
                    Date = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen ?? contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen,
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
}
