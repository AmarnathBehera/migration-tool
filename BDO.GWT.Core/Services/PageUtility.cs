using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core
{
   
    public class PageUtility : IPageUtility
    {
        private readonly IContentService _contentService;

        public PageUtility(IContentService contentService)
        {
            _contentService = contentService;
        }

        /// <summary>
        /// Builds a side navigation box for a page.
        /// 
        /// It fetches all child pages under the given parent page, sorts them by their
        /// display order, and turns each one into a link (title + URL).
        /// The result is a <see cref="SideBoxDto"/> containing the box heading and the list of links,
        /// ready to be rendered as a side navigation menu.
        /// </summary>
        /// <param name="sideboxTitle">The heading shown at the top of the side box.</param>
        /// <param name="parentWebPageItemId">The ID of the parent page whose children become the links.</param>
        /// <param name="titleSelector">A function that picks the link text from each child page.</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>

        public async Task<SideBoxDto> GetSideboxDtoAsync<TPage>(
            string sideboxTitle,
            int parentWebPageItemId,
            Func<TPage, string> titleSelector,
            CancellationToken cancellationToken = default)
            where TPage : class, IWebPageFieldsSource
        {
            var childPages = await _contentService.GetChildPagesByParentGuidAsync<TPage>(
                parentWebPageItemId,
                cancellationToken);

            var linkDtos = childPages
                .OrderBy(p => p.SystemFields.WebPageItemOrder)
                .Select(p => new LinkItemDto
                {
                    Title = titleSelector(p),
                    URL = p.GetUrl().RelativePath,
                });

            return new SideBoxDto
            {
                Title = sideboxTitle,
                Links = linkDtos,
            };
        }

        /// <summary>
        /// Builds an array of multi-link cards for a grid section.
        ///
        /// Each card represents a <typeparamref name="TCategory"/> page (e.g. an Industry Category).
        /// Under each card, "Read More" links are populated from its <typeparamref name="TChild"/> pages
        /// (e.g. individual Industries).
        ///
        /// Steps:
        /// 1. Fetch all category pages under the given parent and sort them by display order.
        /// 2. Fetch all child pages that belong to any of those categories in one query.
        /// 3. Match each child back to its parent category using the parent ID.
        /// 4. Build a card per category, attaching the matched children as "Read More" links.
        /// </summary>
        /// <param name="parentWebPageItemId">The ID of the top-level page that owns the categories.</param>
        /// <param name="cardTitleSelector">Returns the card's title from a category page.</param>
        /// <param name="cardDescriptionSelector">Returns the card's description from a category page.</param>
        /// <param name="cardImageUrlSelector">Returns the card's image URL from a category page (nullable).</param>
        /// <param name="cardImageAltTextSelector">Returns the card image alt text from a category page (nullable).</param>
        /// <param name="childTitleSelector">Returns the link text for each child page.</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>

        public async Task<MultiLinkCardDto[]> GetMultiLinkCardsAsync<TCategory, TChild>(
           int parentWebPageItemId,
           Func<TCategory, string> cardTitleSelector,
           Func<TCategory, string> cardDescriptionSelector,
           Func<TCategory, string?> cardImageUrlSelector,
           Func<TCategory, string?> cardImageAltTextSelector,
           Func<TChild, string> childTitleSelector,
           CancellationToken cancellationToken = default)
           where TCategory : class, IWebPageFieldsSource
           where TChild : class, IWebPageFieldsSource
        {
            var categories = await _contentService.GetChildPagesByParentGuidAsync<TCategory>(
                parentWebPageItemId,
                cancellationToken);

            var orderedCategories = categories
                .OrderBy(c => c.SystemFields.WebPageItemOrder)
                .ToList();

            var categoryIds = orderedCategories
                .Select(c => c.SystemFields.WebPageItemID)
                .ToList();

            var allChildren = new List<TChild>();

            if (categoryIds.Any())
            {
                var childResult = await _contentService.GetPagesAsync<TChild>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
                    query => query
                        .Where(w => w.WhereIn(
                            nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
                            categoryIds))
                        .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                allChildren = childResult
                    .OrderBy(c => c.SystemFields.WebPageItemOrder)
                    .ToList();
            }

            var childUrls = allChildren
                .Select(child => (
                    ParentID: child.SystemFields.WebPageItemParentID,
                    Title: childTitleSelector(child),
                    Url: child.SystemFields.WebPageUrlPath
                ))
                .ToList();

            var cards = orderedCategories
                .Select(category =>
                {
                    var readMoreLinks = childUrls
                        .Where(u => u.ParentID == category.SystemFields.WebPageItemID)
                        .Select(u => new LinkItemDto
                        {
                            Title = u.Title,
                            URL = u.Url
                        })
                        .ToArray();

                    return new MultiLinkCardDto
                    {
                        Title = cardTitleSelector(category),
                        Description = cardDescriptionSelector(category),
                        ImageURL = cardImageUrlSelector(category),
                        ImageAltText = cardImageAltTextSelector(category),
                        Url = category.SystemFields.WebPageUrlPath,
                        ReadMoreLinks = readMoreLinks,
                    };
                })
                .ToArray();

            return cards;
        }
    }
}

