using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core
{
   
    public class PageUtility : IPageUtility
    {
        private readonly IContentService _contentService;
        private readonly IWebPageUrlResolver _urlResolver;

        public PageUtility(IContentService contentService, IWebPageUrlResolver urlResolver)
        {
            _contentService = contentService;
            _urlResolver = urlResolver;
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

        public async Task<SideBoxDto> GetSideboxDtoAsync<TChild, TSibling>(
            string sideboxTitle,
            int parentWebPageItemId,
            int siblingParentWebPageItemId,
            Func<TChild, string> childTitleSelector,
            Func<TSibling, string> siblingTitleSelector,
            CancellationToken cancellationToken = default)
            where TChild : class, IWebPageFieldsSource
            where TSibling : class, IWebPageFieldsSource
        {
            var childPages = await _contentService.GetChildPagesByParentGuidAsync<TChild>(
                parentWebPageItemId,
                cancellationToken);

            var orderedChildren = childPages
                .OrderBy(p => p.SystemFields.WebPageItemOrder)
                .ToList();

            if (orderedChildren.Count > 0)
                {
                return new SideBoxDto
                {
                    Title = sideboxTitle,
                    Links = orderedChildren.Select(p => new LinkItemDto
                    {
                        Title = childTitleSelector(p),
                        URL = _urlResolver.GetRelativePath(p),
                    }),
                };
            }

            var siblingPages = await _contentService.GetChildPagesByParentGuidAsync<TSibling>(
                siblingParentWebPageItemId,
                cancellationToken);

            var orderedSiblings = siblingPages
                .OrderBy(p => p.SystemFields.WebPageItemOrder)
                .ToList();

            if (orderedSiblings.Count == 1)
            {
                orderedSiblings = new List<TSibling>();
            }

            return new SideBoxDto
            {
                Title = sideboxTitle,
                Links = orderedSiblings.Select(p => new LinkItemDto
                {
                    Title = siblingTitleSelector(p),
                    URL = _urlResolver.GetRelativePath(p),
                }),
            };
        }


        public async Task<SideBoxDto> GetSideboxDtoAsync<TSibling>(
            string sideboxTitle,
            int parentWebPageItemId,
            int siblingParentWebPageItemId,
            Func<TSibling, string> siblingTitleSelector,
            CancellationToken cancellationToken = default)
            where TSibling : class, IWebPageFieldsSource
        {


            var siblingPages = await _contentService.GetChildPagesByParentGuidAsync<TSibling>(
                siblingParentWebPageItemId,
                cancellationToken);

            var orderedSiblings = siblingPages
                .OrderBy(p => p.SystemFields.WebPageItemOrder)
                .ToList();

            if (orderedSiblings.Count == 1)
            {
                orderedSiblings = new List<TSibling>();
            }

            return new SideBoxDto
            {
                Title = sideboxTitle,
                Links = orderedSiblings.Select(p => new LinkItemDto
                {
                    Title = siblingTitleSelector(p),
                    URL = _urlResolver.GetRelativePath(p),
                }),
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

            if (categoryIds.Count > 0)
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
                    Url: _urlResolver.GetRelativePath(child)
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
                        Url = _urlResolver.GetRelativePath(category),
                        ReadMoreLinks = readMoreLinks,
                    };
                })
                .ToArray();

            return cards;
        }

        /// <summary>
        /// Builds an array of multi-link cards for a grid section, supporting two types of child pages.
        ///
        /// Each card represents a <typeparamref name="TCategory"/> page (e.g. an Industry Category).
        /// Under each card, "Read More" links are populated from its <typeparamref name="TChild1"/> and
        /// <typeparamref name="TChild2"/> pages (e.g. individual Industries and Reports).
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
        /// <param name="child1TitleSelector">Returns the link text for each child1 page.</param>
        /// <param name="child2TitleSelector">Returns the link text for each child2 page.</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>

        public async Task<MultiLinkCardDto[]> GetMultiLinkCardsAsync2<TCategory, TChild1, TChild2>(
            int parentWebPageItemId,
            Func<TCategory, string> cardTitleSelector,
            Func<TCategory, string> cardDescriptionSelector,
            Func<TCategory, string?> cardImageUrlSelector,
            Func<TCategory, string?> cardImageAltTextSelector,
            Func<TChild1, string> child1TitleSelector,
            Func<TChild2, string> child2TitleSelector,
            CancellationToken cancellationToken = default)
            where TCategory : class, IWebPageFieldsSource
            where TChild1 : class, IWebPageFieldsSource
            where TChild2 : class, IWebPageFieldsSource
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

            var allChildren1 = new List<TChild1>();
            var allChildren2 = new List<TChild2>();

            if (categoryIds.Count > 0)
            {
                var child1Result = await _contentService.GetPagesAsync<TChild1>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
                    query => query
                        .Where(w => w.WhereIn(
                            nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
                            categoryIds))
                        .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                allChildren1 = child1Result
                    .OrderBy(c => c.SystemFields.WebPageItemOrder)
                    .ToList();

                var child2Result = await _contentService.GetPagesAsync<TChild2>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
                    query => query
                        .Where(w => w.WhereIn(
                            nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
                            categoryIds))
                        .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                allChildren2 = child2Result
                    .OrderBy(c => c.SystemFields.WebPageItemOrder)
                    .ToList();
            }

            var childUrls1 = allChildren1
                .Select(child => (
                    ParentID: child.SystemFields.WebPageItemParentID,
                    Title: child1TitleSelector(child),
                    Url: _urlResolver.GetRelativePath(child),
                    Order: child.SystemFields.WebPageItemOrder
                ))
                .ToList();

            var childUrls2 = allChildren2
                .Select(child => (
                    ParentID: child.SystemFields.WebPageItemParentID,
                    Title: child2TitleSelector(child),
                    Url: _urlResolver.GetRelativePath(child),
                    Order: child.SystemFields.WebPageItemOrder
                ))
                .ToList();

            var combinedChildUrls = childUrls1
                .Concat(childUrls2)
                .OrderBy(c => c.Order)
                .ToList();

            var cards = orderedCategories
                .Select(category =>
                {
                    var readMoreLinks = combinedChildUrls
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
                        Url = _urlResolver.GetRelativePath(category),
                        ReadMoreLinks = readMoreLinks,
                    };
                })
                .ToArray();

            return cards;
        }
    }
}

