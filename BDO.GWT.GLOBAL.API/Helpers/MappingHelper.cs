using AngleSharp.Dom;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Helpers
{
    public interface IMappingHelper
    {
        Task<List<ContentItemReference>> BuildContentAttachmentImageReferences(string? url, CancellationToken cancellationToken);
        Task<List<ContentItemReference>> BuildContentAttachmentPersonPageReferences(string? pagePath, string? channelName, string? culture, CancellationToken cancellationToken);

        Task<List<ContentItemReference>> BuildContentAttachmentImageReferencesExtGuid(string? url, CancellationToken cancellationToken);

        /// <summary>
        /// Resolves references to web pages of type <typeparamref name="TPage"/> from their
        /// <c>WebPageItemID</c> values. Used by multi-select web page reference fields such as
        /// <c>MetadataRelevantContactPerson</c> and <c>MetadataOfficeLocation</c>.
        /// </summary>
        Task<List<ContentItemReference>> BuildWebPageIdReferencesAsync<TPage>(int[]? webPageItemIds, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource;

        Task<List<ContentItemReference>> BuildWebPageSingleIdReferencesAsync<TPage>(int? webPageItemId, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource;

        /// <summary>
        /// Resolves a reference to a reusable content item of type <typeparamref name="TItem"/> by its
        /// <c>Name</c> field. Accepts a raw content item GUID directly. Used by fields such as
        /// <c>CareerJobTitle</c>, <c>CareerLevel</c>, and <c>CareerEmploymentType</c>.
        /// </summary>
        Task<List<ContentItemReference>> BuildReusableContentItemNameReferenceAsync<TItem>(string? name, CancellationToken cancellationToken)
            where TItem : class, IContentItemFieldsSource;

        /// <summary>
        /// Bulk counterpart to <see cref="BuildReusableContentItemNameReferenceAsync{TItem}"/>. Selects
        /// reusable content items of type <typeparamref name="TItem"/> whose name matches the supplied
        /// identifiers and returns them as a list of <see cref="ContentItemReference"/>. Names that do
        /// not match any existing item are ignored.
        /// </summary>
        Task<List<ContentItemReference>> BuildReusableContentItemReferencesAsync<TItem>(
            IEnumerable<string>? identifiers, Func<TItem, string> nameSelector,
            CancellationToken cancellationToken) where TItem : class, IContentItemFieldsSource;

        Task<List<ContentItemReference>> BuildReferencesAsync<T>(
           IEnumerable<string>? identifiers, Func<T, string> nameSelector,
           CancellationToken cancellationToken) where T : class, IWebPageFieldsSource;
        Task<List<ContentItemReference>> BuildReferencesPageAsync<T>(
           IEnumerable<string>? identifiers, Func<T, string> nameSelector,
           CancellationToken cancellationToken) where T : class, IWebPageFieldsSource;

        Task<List<ContentItemReference>> BuildReferencesCarAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource;

        Task<List<ContentItemReference>> BuildReferencesPageCarAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource;

        Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken);
        LanguageTag? ValidateAndNormalizeCultureAsync(string languageTagString);

        Task<List<ContentItemReference>> BuildWebPageUrlPathReferenceAsync<TPage>(
            string? pagePathOrGuid, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource;

        Task<string?> ResolveWebPageRelatedItemPathAsync(
            WebPageRelatedItem? item, string? channelName, string? culture, CancellationToken cancellationToken);

        Task<List<ContentItemReference>> BuildFeaturedEventOrWebinarReferencesAsync(
            IEnumerable<string>? identifiers, string? channelName, string? culture, CancellationToken cancellationToken);

        Task<List<ContentItemReference>> BuildFeaturedPersonOrInternalReferencesAsync(
            IEnumerable<string>? identifiers, string? channelName, string? culture, CancellationToken cancellationToken);

        /// <summary>
        /// Creates or updates the <c>BDO.CopyInsight</c> reusable content items described by
        /// <paramref name="items"/> and returns references to them, for use in the
        /// <c>Insight.InsightContent</c> linked-items field.
        /// </summary>
        /// 
        Task<List<ContentItemReference>> BuildOrUpdateCopyInsightReferencesAsync(
            List<string>? items, CancellationToken cancellationToken);

    }

    public class MappingHelper : IMappingHelper
    {
        private readonly IContentService _contentRetriever;
        private readonly ICultureService _cultureService;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IValidationState _validationState;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;

        public MappingHelper(IContentService contentRetriever, ICultureService cultureService, IWebsiteChannelContext websiteChannelContext, IValidationState validationState)
        {
            _contentRetriever = contentRetriever;
            _cultureService = cultureService;
            _websiteChannelContext = websiteChannelContext;
            _validationState = validationState;
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

            if (Guid.TryParse(value, out var guid))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
            }

            var attachments = await _contentRetriever.GetItemsAsync<ContentAttachment>(
                builder => builder.Parameters(p => p.Where(w => w.WhereNotNull("ContentAttachmentAsset"))),
                cancellationToken);

            var match = attachments.FirstOrDefault(a => UrlsMatch(a.ContentAttachmentAsset?.Url, value));

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        public async Task<List<ContentItemReference>> BuildContentAttachmentImageReferencesExtGuid(string? url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new List<ContentItemReference>();
            }

            var value = url.Trim();

            if (Guid.TryParse(value, out var guid))
            {
                return await ExistsAsync(guid, cancellationToken)
                    ? new List<ContentItemReference> { new ContentItemReference { Identifier = guid } }
                    : new List<ContentItemReference>();
            }

            var assetGuid = ExtractContentItemGuidFromAssetUrl(value);
            if (assetGuid is not null && await ExistsAsync(assetGuid.Value, cancellationToken))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = assetGuid.Value } };
            }

            var attachments = await _contentRetriever.GetItemsAsync<ContentAttachment>(
                builder => builder.Parameters(p => p.Where(w => w.WhereNotNull("ContentAttachmentAsset"))),
                cancellationToken);

            var match = attachments.FirstOrDefault(a => UrlsMatch(a.ContentAttachmentAsset?.Url, value));

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        private static Guid? ExtractContentItemGuidFromAssetUrl(string url)
        {
            var normalized = url.Trim().TrimStart('~');

            var queryIndex = normalized.IndexOf('?');
            if (queryIndex >= 0)
            {
                normalized = normalized[..queryIndex];
            }

            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var assetIndex = Array.FindIndex(segments, s => string.Equals(s, "getContentAsset", StringComparison.OrdinalIgnoreCase));
            if (assetIndex >= 0 && segments.Length > assetIndex + 1
                && Guid.TryParse(segments[assetIndex + 1], out var contentItemGuid))
            {
                return contentItemGuid;
            }

            return null;
        }

        private async Task<bool> ExistsAsync(Guid contentItemGuid, CancellationToken cancellationToken)
        {
            var items = await _contentRetriever.GetItemsAsync<ContentAttachment>(
                builder => builder.Parameters(p => p.Where(w => w.WhereEquals("ContentItemGUID", contentItemGuid))),
                cancellationToken);

            return items.Any();
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

        /// <inheritdoc />
        public async Task<List<ContentItemReference>> BuildWebPageIdReferencesAsync<TPage>(int[]? webPageItemIds, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource
        {
            if (webPageItemIds is null || webPageItemIds.Length == 0)
            {
                return new List<ContentItemReference>();
            }

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var pages = await _contentRetriever.GetPagesAsync<TPage>(
                parameters,
                query => query.Where(w => w.WhereIn("WebPageItemID", webPageItemIds)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            return pages?
                .Select(p => p.SystemFields.ContentItemGUID)
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList() ?? new List<ContentItemReference>();
        }

        public async Task<List<ContentItemReference>> BuildWebPageSingleIdReferencesAsync<TPage>(int? webPageItemId, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource
        {
            if (webPageItemId is null || webPageItemId == 0)
            {
                return new List<ContentItemReference>();
            }

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var pages = await _contentRetriever.GetPagesAsync<TPage>(
                parameters,
                query => query.Where(w => w.WhereEquals("WebPageItemID", webPageItemId)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            return pages?
                .Select(p => p.SystemFields.ContentItemGUID)
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList() ?? new List<ContentItemReference>();
        }

        /// <inheritdoc />
        public async Task<List<ContentItemReference>> BuildReusableContentItemNameReferenceAsync<TItem>(string? name, CancellationToken cancellationToken)
            where TItem : class, IContentItemFieldsSource
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<ContentItemReference>();
            }

            var value = name.Trim();

            if (Guid.TryParse(value, out var guid))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
            }

            var items = await _contentRetriever.GetItemsAsync<TItem>(
                builder => builder.Parameters(p => p.Where(w => w.WhereEquals("Name", value))),
                cancellationToken);

            var match = items?.FirstOrDefault();

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        /// <inheritdoc />
        public async Task<List<ContentItemReference>> BuildReusableContentItemReferencesAsync<TItem>(
            IEnumerable<string>? identifiers,
            Func<TItem, string> nameSelector,
            CancellationToken cancellationToken)
            where TItem : class, IContentItemFieldsSource
        {
            var names = identifiers?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (names.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var items = await _contentRetriever.GetItemsAsync<TItem>(
                builder => builder.Parameters(p => p.Where(w => w.WhereIn("Name", names))),
                cancellationToken);

            return items
                .Where(item => names.Contains(nameSelector(item), StringComparer.OrdinalIgnoreCase))
                .Select(item => item.SystemFields.ContentItemGUID)
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList();
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

        /// <summary>
        /// Selects the content items of type <typeparamref name="T"/> whose name matches the supplied
        /// identifiers and returns them as a list of <see cref="ContentItemReference"/>. Names that do
        /// not match any existing item are ignored.
        /// </summary>
        public async Task<List<ContentItemReference>> BuildReferencesAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource
        {
            var names = identifiers?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (names.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var items = await _contentRetriever.GetItemsAsync<T>(
                builder => builder.Parameters(p => p.Where(w => w.WhereIn("Name", names))),
                cancellationToken);

            return items
                .Where(item => names.Contains(nameSelector(item), StringComparer.OrdinalIgnoreCase))
                .Select(item => item.SystemFields.ContentItemGUID)
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList();
        }

        public async Task<List<ContentItemReference>> BuildReferencesPageAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource
        {
            var names = identifiers?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (names.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var items = await _contentRetriever.GetItemsAsync<T>(
                builder => builder.Parameters(p => p.Where(w => w.WhereIn("MetadataTitle", names))),
                cancellationToken);

            return items
                .Where(item => names.Contains(nameSelector(item), StringComparer.OrdinalIgnoreCase))
                .Select(item => item.SystemFields.ContentItemGUID)
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList();
        }

        public Task<List<ContentItemReference>> BuildReferencesCarAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource =>
            BuildReferencesAsync(identifiers, nameSelector, cancellationToken);

        public Task<List<ContentItemReference>> BuildReferencesPageCarAsync<T>(
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource =>
            BuildReferencesPageAsync(identifiers, nameSelector, cancellationToken);

        public async Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken)
        {
            var languageTag = ValidateAndNormalizeCultureAsync(languageTagString);

            if (languageTag == null)
            {
                return null;
            }

            var siteCultures = (await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken)).ToHashSet();

            if (!siteCultures.Contains(languageTag))
            {
                _validationState.AddError("culture", ErrorMessage.DocumentCultureNotAllowedOnSite(languageTag, _websiteChannelContext.WebsiteChannelName, siteCultures));
            }

            return languageTag;
        }

        public LanguageTag? ValidateAndNormalizeCultureAsync(string languageTagString)
        {
            if (LanguageTag.TryParse(languageTagString, out var languageTag))
            {
                return languageTag;
            }

            _validationState.AddError("culture", ErrorMessage.IncorrectLanguageTagFormat(languageTagString));

            return null;
        }

        public async Task<List<ContentItemReference>> BuildWebPageUrlPathReferenceAsync<TPage>(
            string? pagePathOrGuid, string? channelName, string? culture, CancellationToken cancellationToken)
            where TPage : class, IWebPageFieldsSource
        {
            if (string.IsNullOrWhiteSpace(pagePathOrGuid))
            {
                return new List<ContentItemReference>();
            }

            var value = pagePathOrGuid.Trim();

            if (Guid.TryParse(value, out var guid))
            {
                return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
            }

            var normalizedPath = value.TrimStart('~');
            var queryIndex = normalizedPath.IndexOf('?');
            if (queryIndex >= 0) normalizedPath = normalizedPath[..queryIndex];
            normalizedPath = normalizedPath.TrimEnd('/');

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var pages = await _contentRetriever.GetPagesAsync<TPage>(
                parameters,
                query => query.Where(w => w.WhereEquals("WebPageUrlPath", normalizedPath)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var match = pages?.FirstOrDefault();

            return match is null
                ? new List<ContentItemReference>()
                : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
        }

        public async Task<string?> ResolveWebPageRelatedItemPathAsync(
            WebPageRelatedItem? item, string? channelName, string? culture, CancellationToken cancellationToken)
        {
            if (item is null)
            {
                return null;
            }

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var pages = await _contentRetriever.GetPagesAsync<SectionEvents>(
                parameters,
                query => query.Where(w => w.WhereEquals("WebPageItemGUID", item.WebPageGuid)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            return pages?.FirstOrDefault()?.SystemFields?.WebPageItemTreePath;
        }

        public async Task<List<ContentItemReference>> BuildFeaturedEventOrWebinarReferencesAsync(
            IEnumerable<string>? identifiers, string? channelName, string? culture, CancellationToken cancellationToken)
        {
            var names = identifiers?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (names.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var eventPages = await _contentRetriever.GetPagesAsync<Event>(
                parameters,
                query => query.Where(w => w.WhereIn("MetadataTitle", names)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var webinarPages = await _contentRetriever.GetPagesAsync<Webinar>(
                parameters,
                query => query.Where(w => w.WhereIn("MetadataTitle", names)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var guids = new List<Guid>();

            if (eventPages is not null)
            {
                guids.AddRange(eventPages.Select(p => p.SystemFields.ContentItemGUID));
            }

            if (webinarPages is not null)
            {
                guids.AddRange(webinarPages.Select(p => p.SystemFields.ContentItemGUID));
            }

            return guids
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList();
        }

        public async Task<List<ContentItemReference>> BuildFeaturedPersonOrInternalReferencesAsync(
            IEnumerable<string>? identifiers, string? channelName, string? culture, CancellationToken cancellationToken)
        {
            var names = identifiers?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (names.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var parameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 0,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            var personPages = await _contentRetriever.GetPagesAsync<Person>(
                parameters,
                query => query.Where(w => w.WhereIn("MetadataTitle", names)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var internalPersonPages = await _contentRetriever.GetPagesAsync<InternalPerson>(
                parameters,
                query => query.Where(w => w.WhereIn("MetadataTitle", names)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var guids = new List<Guid>();

            if (personPages is not null)
            {
                guids.AddRange(personPages.Select(p => p.SystemFields.ContentItemGUID));
            }

            if (internalPersonPages is not null)
            {
                guids.AddRange(internalPersonPages.Select(p => p.SystemFields.ContentItemGUID));
            }

            return guids
                .Distinct()
                .Select(guid => new ContentItemReference { Identifier = guid })
                .ToList();
        }
        public async Task<List<ContentItemReference>> BuildOrUpdateCopyInsightReferencesAsync(
            List<string>? items, CancellationToken cancellationToken)
        {
            if (items is null || items.Count == 0)
            {
                return new List<ContentItemReference>();
            }

            var result = new List<ContentItemReference>();

            var matches = (await _contentRetriever.GetItemsAsync<CopyInsight>(
                    builder => builder.Parameters(p => p.Where(w => w.WhereEquals("Title", items[0]))),
                    cancellationToken))?.ToList();

            var match = matches is { Count: > 0 } ? matches[0] : null;

            if (match is not null)
            {
                result.Add(new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID });
            }

            return result;
        }
    }
}