using BDO.GWT.Core;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Helpers
{
    public interface IMappingHelper
    {
        Task<List<ContentItemReference>> BuildContentAttachmentImageReferences(string? url, CancellationToken cancellationToken);
        Task<List<ContentItemReference>> BuildContentAttachmentPersonPageReferences(string? pagePath, string? channelName, string? culture, CancellationToken cancellationToken);
    }
    public class MappingHelper : IMappingHelper
    {
        private readonly IContentService _contentRetriever;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;

        public MappingHelper(IContentService contentRetriever)
        {
            _contentRetriever = contentRetriever;
        }

        /// <summary>
        /// Resolves a Content hub image reference from an image URL. Retrieves <c>ContentAttachment</c>
        /// content items and matches the one whose <c>ContentAttachmentAsset</c> URL corresponds to the
        /// supplied value, returning it as a list of <see cref="ContentItemReference"/>. A raw content
        /// item GUID is also accepted. Values that do not match any existing item result in an empty
        /// (cleared) field.
        /// </summary>
        public async Task<List<ContentItemReference>> BuildContentAttachmentImageReferences(string? url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new List<ContentItemReference>();
            }

            var value = url.Trim();

            // Accept a raw content item GUID directly.
            if (Guid.TryParse(value, out var guid))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
            }

            // Otherwise treat the value as an image URL and look up the matching ContentAttachment.
            var attachments = await _contentRetriever.GetItemsAsync<ContentAttachment>(
                builder => builder.Parameters(p => p.Where(w => w.WhereNotNull("ContentAttachmentAsset"))),
                cancellationToken);

            var match = attachments.FirstOrDefault(a => UrlsMatch(a.ContentAttachmentAsset?.Url, value));

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        /// <summary>
        /// Resolves a web page reference from its <c>WebPageUrlPath</c>. Accepts a raw content item GUID
        /// or a web page URL path, looks up the matching page in the current channel, and returns it as a
        /// list of <see cref="ContentItemReference"/>. Used by web page reference fields such as
        /// <c>RelevantContactPersonDeal</c> and <c>HeroCallToActionButtonUrl</c>. Values that do not match
        /// any existing page result in an empty (cleared) field.
        /// </summary>
        public async Task<List<ContentItemReference>> BuildContentAttachmentPersonPageReferences(string? pagePath, string? channelName, string? culture, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(pagePath))
            {
                return new List<ContentItemReference>();
            }

            var value = pagePath.Trim();

            // Accept a raw content item GUID directly.
            if (Guid.TryParse(value, out var guid))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
            }

            var normalizedPath = value.TrimStart('~');
            var queryIndex = normalizedPath.IndexOf('?');
            if (queryIndex >= 0)
            {
                normalizedPath = normalizedPath[..queryIndex];
            }
            normalizedPath = normalizedPath.TrimEnd('/');

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var pages = await _contentRetriever.GetPagesAsync<Person>(
                parameters,
                query => query.Where(w => w.WhereEquals("WebPageUrlPath", normalizedPath)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var match = pages?.FirstOrDefault();

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        /// <summary>
        /// Compares two image URLs for equality, ignoring case, a leading application-relative marker
        /// (<c>~</c>), and any query string. Falls back to comparing the final path segment (file name).
        /// </summary>
        private static bool UrlsMatch(string? assetUrl, string? requestedUrl)
        {
            if (string.IsNullOrWhiteSpace(assetUrl) || string.IsNullOrWhiteSpace(requestedUrl))
            {
                return false;
            }

            static string Normalize(string url)
            {
                var normalized = url.Trim().TrimStart('~');

                var queryIndex = normalized.IndexOf('?');
                if (queryIndex >= 0)
                {
                    normalized = normalized[..queryIndex];
                }

                return normalized.TrimEnd('/');
            }

            var normalizedAsset = Normalize(assetUrl);
            var normalizedRequested = Normalize(requestedUrl);

            if (string.Equals(normalizedAsset, normalizedRequested, StringComparison.OrdinalIgnoreCase)
                || normalizedAsset.EndsWith(normalizedRequested, StringComparison.OrdinalIgnoreCase)
                || normalizedRequested.EndsWith(normalizedAsset, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            static string FileName(string url) => url[(url.LastIndexOf('/') + 1)..];

            return string.Equals(FileName(normalizedAsset), FileName(normalizedRequested), StringComparison.OrdinalIgnoreCase);
        }
    }
}
