using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
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
        Task<FilterItemDto<OptionDto>[]> CreateBlogPageFiltersAsync(HashSet<string> filters,int pageId, CancellationToken cancellationToken);
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
       
        private readonly ICultureContext _cultureContext;
        private readonly IContentService _contentService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IWebPageUrlResolver _urlResolver;
        private int currentBlogsPageId;
        public BlogPageCardsService(
            ICardsService cardsService,
            ICultureContext cultureContext,
            IContentService contentService,
            IUrlHelperFactory urlHelperFactory,
            IHttpContextAccessor httpContextAccessor,
            IWebsiteChannelContext websiteChannelContext,
            IWebPageUrlResolver urlResolver
            )
        {
           
            _cultureContext = cultureContext;
            _contentService = contentService;
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
            _websiteChannelContext = websiteChannelContext;
            _urlResolver = urlResolver;
        }

       

        public async Task<FilterItemDto<OptionDto>[]> CreateBlogPageFiltersAsync(HashSet<string> filters,int pageId,  CancellationToken cancellationToken)
        {
            currentBlogsPageId = pageId;
            return await CreateBlogFiltersAsync(filters, cancellationToken);
        }

        private async Task<FilterItemDto<OptionDto>[]> CreateBlogFiltersAsync(HashSet<string> filterParameterNames,  CancellationToken cancellationToken)
        {
            var filterItems = new List<FilterItemDto<OptionDto>>();

            foreach (var filterParameterName in filterParameterNames)
            {
                var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

                if (filterParameterData is null)
                {
                    continue;
                }

                var options = filterParameterData.FilterParameterType switch
                {
                    FilterParameterType.Page => await BuildOptionsForPageFilterAsync(filterParameterData, cancellationToken),
                    FilterParameterType.Tagpage => await BuildOptionsForTagPageFilterAsync(filterParameterData, cancellationToken),

                    // FilterParameterType.Entity => await BuildOptionsForEntityFilterAsync(filterParameterData, cancellationToken),
                    FilterParameterType.Field => await BuildOptionsForFieldFilterAsync(filterParameterData, cancellationToken),
                    //FilterParameterType.Taxonomy => await BuildOptionsForTaxonomyFilterAsync(filterParameterData, cancellationToken),
                    _ => throw new NotSupportedException($"Filter parameter type {filterParameterData.FilterParameterType} is not supported."),
                };

                var filterItem = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, filterParameterName, filterParameterData.LiveSiteDisplayName, null, options);

                filterItems.Add(filterItem);
            }

            return filterItems.ToArray();
        }
        private async Task<OptionDto[]> BuildOptionsForPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
        {
            var entityType = filterParameterData.EntityType;

            if(entityType.Name == "Person")
            {
                var blogContributorPages = await _contentService.GetChildPagesByParentGuidAsync<BlogContributors>(currentBlogsPageId, cancellationToken);
                List<Person> contributorPersons = new List<Person>();
                foreach (var blogContributorPage in blogContributorPages)
                {
                    var blogContributorPersonPages = await _contentService.GetChildPagesByParentGuidAsync<Person>(blogContributorPage.SystemFields.WebPageItemID, cancellationToken);
                    contributorPersons.AddRange(blogContributorPersonPages);
                }

                if(contributorPersons.Count > 0)
                {
                    return contributorPersons
                        .Select(x =>
                        {
                            var name = $"{x.PersonFirstName} {x.PersonLastName}".Trim();
                            return new OptionDto
                            {
                                Value = name,
                                Label = name
                            };
                        })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .OrderBy(x => x.Label)
                        .ToArray();
                }
                 else
                {
                    return Array.Empty<OptionDto>();
                }
            }
                  

            var items = await _contentService.GetItemsByTypesAsync(
                new[] { entityType },
                _websiteChannelContext.WebsiteChannelID,
                cancellationToken);

            if(items == null)
            {
                return Array.Empty<OptionDto>();
            }

            var options = items
      .Select(x =>
      {
          var type = x.GetType();

          var metadataTitle =
              type.GetProperty("MetadataTitle")?.GetValue(x) as string;

          var metadataOgTitle =
              type.GetProperty("MetadataOGTitle")?.GetValue(x) as string;

          var systemFields =
              type.GetProperty("SystemFields")?.GetValue(x);

          var contentItemName =
              systemFields?.GetType()
                  .GetProperty("ContentItemName")
                  ?.GetValue(systemFields) as string;

          var name = metadataTitle ?? metadataOgTitle ?? contentItemName;

          return new OptionDto
          {
              Value = name,
              Label = name
          };
      })
      .Where(x => !string.IsNullOrWhiteSpace(x.Value))
      .OrderBy(x => x.Label)
      .ToArray();

            return options;
        }

        private async Task<OptionDto[]> BuildOptionsForTagPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
        {
            var entityType = filterParameterData.EntityType;

            var items = await _contentService.GetItemsByTypesAsync(
                new[] { entityType },
                _websiteChannelContext.WebsiteChannelID,
                cancellationToken);

            var options = items
                        .Select(x =>
                        {
                            var type = x.GetType();

                            var tagNameValue = type.GetProperty("Name")?.GetValue(x);

                            string name = tagNameValue is string s ? s : string.Empty;


                            return new OptionDto
                            {
                                Value = name,
                                Label = name


                            };
                        })
                              .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                              .OrderBy(x => x.Label)
                              .ToArray();

            return options;
        }

        private async Task<OptionDto[]> BuildOptionsForFieldFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
        {
            var entityType = filterParameterData.EntityType;
            var options = Array.Empty<OptionDto>();

            var items = await _contentService.GetItemsByTypesAsync(
                new[] { entityType },
                _websiteChannelContext.WebsiteChannelID,
                cancellationToken);

            if (items is null || !items.Any())
            {
                return options;
            }

            switch (filterParameterData.EntityType)
            {
                case { } type when type == typeof(BlogPost):
                    options = items
                        .Select(x => x.GetType().GetProperty(nameof(BlogPost.MetadataKeywordsBlog))?.GetValue(x) as string)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .SelectMany(x => KeywordsHelper.ParseKeywords(x!))
                        .Distinct()
                        .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
                        .ToArray();
                    break;
            }

            return options;
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

            //Get blog groups under the parent page Blogs, to then get the blog posts under each group.
            //This is needed to apply the parent page filter for blog posts if provided in the query parameters.
            //as each blog post is under a blog group and not directly under the Blogs parent page.
            var blogPostsGroups = await _contentService.GetChildPagesByParentGuidAsync<BlogPostGroup>(parentPageId, cancellationToken);
                        
            List<BlogPost> blogPostsList = new List<BlogPost>();

            foreach (var blogPostGrp in blogPostsGroups)
            {
                var blogPosts = await _contentService.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                                   query =>
                                    {
                                         // Filter by specific language
                                         query.InLanguage(language);

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
                FilterParameterName.MicrositePage => x => x.WhereContains(nameof(BlogPost.MicrositePages), guid),
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
                        URL = _urlResolver.GetAbsoluteUrl(contentPage)
                    }
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
