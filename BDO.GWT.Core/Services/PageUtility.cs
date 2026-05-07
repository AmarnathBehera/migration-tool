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
    /// <summary>
    /// XbyK implementation of <see cref="IPageUtility"/>.
    /// Wraps <see cref="IContentService"/> to provide reusable page utility operations.
    /// </summary>
    public class PageUtility : IPageUtility
    {
        private readonly IContentService _contentService;

        public PageUtility(IContentService contentService)
        {
            _contentService = contentService;
        }

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
            // ── 1. Load category pages — direct children of parent ────────────
            var categories = await _contentService.GetChildPagesByParentGuidAsync<TCategory>(
                parentWebPageItemId,
                cancellationToken);

            var orderedCategories = categories
                .OrderBy(c => c.SystemFields.WebPageItemOrder)
                .ToList();

            // ── 2. Load ALL child pages across all categories in one query ────
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

            // ── 3. Build flat child lookup keyed by ParentID ──────────────────
            var childUrls = allChildren
                .Select(child => (
                    ParentID: child.SystemFields.WebPageItemParentID,
                    Title: childTitleSelector(child),
                    Url: child.SystemFields.WebPageUrlPath
                ))
                .ToList();

            // ── 4. Build one card per category ────────────────────────────────
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

