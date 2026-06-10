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
using System.Linq.Expressions;

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
            int parentPageId,
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
            int parentPageId,
            CancellationToken cancellationToken)
        {
            var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

            var blogPostsGroups = await _contentService.GetChildPagesByParentGuidAsync<BlogPostGroup>(parentPageId, cancellationToken);

            foreach (var blogPostGroup in blogPostsGroups)
            {
                var blogPostList = await _contentService.GetChildPagesByParentGuidAsync<BlogPost>(blogPostGroup.SystemFields.WebPageItemID, cancellationToken);
            }

            List<BlogPost> blogPostsList = new List<BlogPost>();

            foreach (var blogPostGrp in blogPostsGroups)
            {
                var blogPosts = await _contentService.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                                   query =>
                                    {
                                         // Apply parent ID filter if provided
                                          if (parentPageId > 0)
                                          {
                                              query.Parameters(p => p.Where(w => w.WhereEquals(nameof(BlogPost.SystemFields.WebPageItemParentID), blogPostGrp.SystemFields.WebPageItemID)));
                                          }

                                          // Apply filter expressions
                                          foreach (var filterExpression in filterWhereExpressions)
                                          {
                                              query.Parameters(p => p.Where(filterExpression));
                                          }

                                          // Apply any additional query decorations
                                          decorateQuery?.Invoke(query);
                                    },
                                    cancellationToken);
                 

                blogPostsList.AddRange(blogPosts);
            }
          

            var orderedBlogPosts = blogPostsList
                .Where(x => x is not null)
                .Cast<BlogPost>()
                .OrderByDescending(isFeatured)
                .ThenByDescending(x => x.SystemFields.ContentItemCommonDataLastPublishedWhen
                                    ?? x.SystemFields.ContentItemCommonDataFirstPublishedWhen)
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
                TotalCount = orderedBlogPosts.Length,
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
                    FilterParameterType.Field => await GetFieldFilterExpressionBlogAsync(filterParameterName, filterParameterValue),
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
                FilterParameterName.BusinessIssue => x => x.WhereContains(nameof(BlogPost.MetadataBusinessIssues), guid),
                FilterParameterName.ServiceArea => x => x.WhereContains(nameof(BlogPost.MetadataServiceAreas), guid),
                FilterParameterName.Service => x => x.WhereContains(nameof(BlogPost.MetadataServices), guid),
                FilterParameterName.IndustryCategory => x => x.WhereContains(nameof(BlogPost.MetadataIndustryCategories), guid),
                FilterParameterName.IndustryPage => x => x.WhereContains(nameof(BlogPost.MetadataIndustries), guid),
                FilterParameterName.MicrositeMasterPage => x => x.WhereContains(nameof(BlogPost.MicrositeMasterPages), guid),
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
               
                FilterParameterName.BusinessIssue => x => x.WhereContains(nameof(BlogPost.MetadataBusinessIssues), guid),
               
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
                var type = contentPage.ContentType.FirstOrDefault();
                string typeConst = string.Empty;
               
                if(type != null)
                {
                    typeConst = type.Name;
                }
                    
                bool isComingSoon = false;
                var card = new BlogPageCardDto
                {
                    IsFeatured = isPageFeatured(contentPage),
                    ImageURL = urlHelper.Content(contentPage.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset?.Url ?? string.Empty),
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
                        URL = contentPage.SystemFields.WebPageUrlPath                    }
                };

                if(card != null)
                {
                    cards.Add(card);
                }
                
            }
            return cards;
        }

        protected async Task<Action<WhereParameters>?> GetFieldFilterExpressionBlogAsync(string filterParameterName, string filterParameterValue)
        {
            if (filterParameterName == FilterParameterName.Keywords)
            {
                return (x => x.WhereContains(nameof(BlogPost.MetadataKeywordsBlog), filterParameterValue));
            }

            return null;
        }
    }
}
