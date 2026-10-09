using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Helpers
{
    public interface IContentPageCardHelper<TPage> where TPage : IWebPageFieldsSource, IMetadata
    {
        Task<List<ContentPageCardDto>> GetContentPageCardListAsync(
            ICollection<TPage> contentPages,
            LanguageTag languageTag,
            Func<TPage, bool> isPageFeatured,
            CancellationToken cancellationToken);
    }

    public class ContentPageCardHelper<TPage> : IContentPageCardHelper<TPage>
        where TPage : IWebPageFieldsSource, IMetadata
    {
        private readonly IPageUrlService _pageUrlService;

        public ContentPageCardHelper(IPageUrlService pageUrlService)
        {
            _pageUrlService = pageUrlService;
        }

        public async Task<List<ContentPageCardDto>> GetContentPageCardListAsync(
            ICollection<TPage> contentPages,
            LanguageTag languageTag,
            Func<TPage, bool> isPageFeatured,
            CancellationToken cancellationToken)
        {
            var cards = new List<ContentPageCardDto>();

            var webPageItemIds = contentPages.Select(x => x.SystemFields.WebPageItemID);

            var urlByWebPageItemId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(
                webPageItemIds, languageTag.Locale, cancellationToken);

            foreach (var contentPage in contentPages)
            {
                var webPageItemId = contentPage.SystemFields.WebPageItemID;

                bool isComingSoon = contentPage is Insight insightPage && insightPage.ComingSoon;

                urlByWebPageItemId.TryGetValue(webPageItemId, out var relativeLink);

                var card = new ContentPageCardDto
                {
                    IsFeatured = isPageFeatured(contentPage),
                    ImageAltText = contentPage.MetadataTeaserImageAltText,
                    Title = contentPage.MetadataTitle,
                    Date = contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                    UpdatedOnDate = GetUpdatedOn(contentPage),
                    Description = HtmlHelper.ExtractTextFromHtml(contentPage.MetadataDescription),
                    ReadMoreLink = new LinkItemDto
                    {
                        Title = ResourceStringKey.LiveSite.ReadMore,
                        URL = relativeLink?.ToHtmlCompatibleString() ?? "#",
                    },
                };

                cards.Add(card);
            }

            return cards;
        }

        private static DateTime? GetUpdatedOn(TPage contentPage)
        {
            if (contentPage is IContentPage page)
            {
                return page.UpdatedOn == DateTime.MinValue ? null : page.UpdatedOn;
            }

            return null;
        }
    }
}
