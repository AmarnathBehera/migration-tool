using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.Services
{
    public interface IBlogsPageService
    {
        Task<BlogPageDto?> GetBlogsPageDtoAsync(Blog currentPage, CancellationToken cancellationToken);
        Task<PaginationDto<BlogPageCardDto?>> GetBlogsPaginationResultAsync(int pageId,string language, CardsQuery cardsQuery, CancellationToken cancellationToken);
    }
    public class BlogsPageService : IBlogsPageService
    {

        private readonly IFilteringHelper _filteringHelper;
        private readonly IHeroService _heroService;
        private readonly IBlogPageCardsService _blogPageCardsService;
        private readonly IContentService _contentService;
        private readonly IContentRetriever _contentRetriever;
       
        public BlogsPageService(IFilteringHelper filteringHelper, IHeroService heroService, IBlogPageCardsService blogPageCardsService, IContentService contentService, IContentRetriever contentRetriever)
        {
            _filteringHelper = filteringHelper;
            _heroService = heroService;
            _blogPageCardsService = blogPageCardsService;
            _contentService = contentService;
            _contentRetriever = contentRetriever;
        }

        public async Task<BlogPageDto?> GetBlogsPageDtoAsync(Blog currentPage, CancellationToken cancellationToken)
        {
            if (currentPage == null)
            {
                return null;
            }

           

            bool enableFilters = currentPage.ItemListFilteringEnabled;

            var bgImageUrl = currentPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var mobileImageUrl = currentPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var ctaUrlPath = currentPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;
            var heroData = new HeroData
            {
                Layout = currentPage.HeroLayout,
                Title = currentPage.HeroTitle,
                Description = currentPage.HeroDescription,
                BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
                BackgroundImageAltText = currentPage.HeroBackgroundImageAltText,
                MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
                MobileBackgroundImageAltText = currentPage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = currentPage.HeroCallToActionButtonType,
                CallToActionButtonText = currentPage.HeroCallToActionButtonText,
                CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
            };

            // Construct the API endpoint URL using the page's GUID which is passed as url parameter to the BlogPostsAPIController to filter blog posts related to the current page
            var fullApiEndpoint = $"BlogPosts/{currentPage.SystemFields.WebPageItemID}";

            BlogPageDto blogPageDtoObj = new BlogPageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                EnableFilters = enableFilters,
               CardsGrid = await GetBlogCardsGridDtoAsync(currentPage, fullApiEndpoint, cancellationToken)

            };

            return blogPageDtoObj;
        }

        private async Task<CardsGridDto> GetBlogCardsGridDtoAsync(Blog currentPage,string apiEndPointName, CancellationToken cancellationToken)
        {
            HashSet<string>? filtersSet = new HashSet<string>();
            bool enableFilters = currentPage.ItemListFilteringEnabled;

            if(enableFilters == true)
            {
                if(!string.IsNullOrWhiteSpace(currentPage.BlogPostListFilters))
                {
                    filtersSet = currentPage.BlogPostListFilters
                       .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToHashSet();
                }                  
            }
                      

            var selectedpageid = currentPage.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.WebPageGuid;
            var selectedPage = selectedpageid.HasValue && selectedpageid.Value != Guid.Empty
                               ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>([selectedpageid.Value], cancellationToken: cancellationToken)
                                : Enumerable.Empty<IWebPageFieldsSource>();

            var blogCardsGridDto = new CardsGridDto
            {
                Title = currentPage.ItemListTitle,
                Description = currentPage.ItemListDescription,
                Filters = enableFilters ? await _blogPageCardsService.CreateBlogPageFiltersAsync(filtersSet, currentPage.SystemFields.WebPageItemID, cancellationToken) : null,
                FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(currentPage.ItemListFilteringEnabled, cancellationToken),
                ApiEndpointName = apiEndPointName,
                PageSize = 20,
                AdvancedSearchPrelinkText = currentPage.ItemListAdvancedSearchDescription,
                AdvancedSearchLinkText = currentPage.ItemListAdvancedSearchLinkText,
                AdvancedSearchLink = selectedPage.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,//?.ToHtmlCompatibleString(),
            };

            return blogCardsGridDto;
        }

        public async Task<PaginationDto<BlogPageCardDto?>> GetBlogsPaginationResultAsync(int pageId, string language, CardsQuery cardsQuery, CancellationToken cancellationToken)
        {
            var currrentBlogPage = (await _contentService.GetPagesAsync<Blog>(
             new RetrievePagesParameters
             {
                 LanguageName = language,
                 UseLanguageFallbacks = false
             },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                .Where(w => w.WhereEquals("WebPageItemID", pageId)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).FirstOrDefault();

            if (currrentBlogPage == null)
            {
                throw new MissingSectionPageException($"'{nameof(Blog)} page with ID '{pageId}' not found.");
            }

            var featuredBlogsPageIds = await GetFeaturedBlogPostPageIdsAsync(currrentBlogPage.SystemFields.ContentItemGUID, cancellationToken);

            var paginationDto = await _blogPageCardsService.GetFilteredItemsAsync(
                language,
                cardsQuery,
                 null,
                blogPost => featuredBlogsPageIds.Contains(blogPost.SystemFields.ContentItemGUID),
                currrentBlogPage.SystemFields.WebPageItemID,
                cancellationToken
                );

            if (paginationDto == null)
            {
               return new PaginationDto<BlogPageCardDto?>();
            }
            
            return new PaginationDto<BlogPageCardDto?>
            {
                Data = paginationDto.Data,
                TotalCount = paginationDto.TotalCount
            };
        }

        private async Task<ISet<Guid>> GetFeaturedBlogPostPageIdsAsync(Guid contentItemGuid, CancellationToken cancellationToken)
        {
            var currentPage = (await _contentService.GetItemAsync<Blog>(contentItemGuid, cancellationToken));

            if (currentPage == null)
            {
                return new HashSet<Guid>();
            }

            var featuredBlogPostPageIds = currentPage.BlogPostListFeaturedItems.Select(x => x.WebPageGuid).ToHashSet();

            return featuredBlogPostPageIds;
        }

    }
}