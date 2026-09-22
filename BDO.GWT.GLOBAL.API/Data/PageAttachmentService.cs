using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.Core;
using CMS.Websites;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Data
{
    public class PageAttachmentService : IPageAttachmentService
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
        private const string EventSource = nameof(PageAttachmentService);

        private readonly IContentService _contentService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAssetUrlResolver _assetUrlResolver;
        private readonly IEventLogService _eventLogService;
        private readonly IChannelWebsiteDomainService _channelWebsiteDomainService;

        public PageAttachmentService(
            IContentService contentService,
            IHttpClientFactory httpClientFactory,
            IAssetUrlResolver assetUrlResolver,
            IEventLogService eventLogService,
            IChannelWebsiteDomainService channelWebsiteDomainService)
        {
            _contentService = contentService;
            _httpClientFactory = httpClientFactory;
            _assetUrlResolver = assetUrlResolver;
            _eventLogService = eventLogService;
            _channelWebsiteDomainService = channelWebsiteDomainService;
        }

        public async Task<IEnumerable<ContentAttachment>> GetPageAttachmentsAsync(
            Guid pageGuid,
            string channelName,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var pages = await _contentService.GetItemsByGUIDForAPIAsync(
                new List<Guid> { pageGuid },
                channelName,
                culture);

            if (pages is null)
            {
                return Array.Empty<ContentAttachment>();
            }

            return pages
                .SelectMany(GetAttachmentsFromPage)
                .ToList();
        }

        public async Task<(byte[] Data, string ContentType, string FileName)?> GetPageAttachmentBinaryAsync(
            Guid pageGuid,
            Guid attachmentGuid,
            string channelName,
            string culture,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var attachments = await GetPageAttachmentsAsync(pageGuid, channelName, culture, cancellationToken);
                var attachment = attachments.FirstOrDefault(x => x.SystemFields.ContentItemGUID == attachmentGuid);
                var asset = attachment?.ContentAttachmentAsset;

                if (asset?.Url is null)
                {
                    _eventLogService.LogWarning(
                        EventSource,
                        nameof(GetPageAttachmentBinaryAsync),
                        $"Attachment binary was skipped because the asset URL was missing. PageGUID: {pageGuid}, AttachmentGUID: {attachmentGuid}, Channel: {channelName}, Culture: {culture}.");
                    return null;
                }

                var websiteDomain = await _channelWebsiteDomainService.GetWebsiteDomainAsync(channelName, cancellationToken);
                var assetUri = _assetUrlResolver.BuildAssetUri(asset.Url, websiteDomain);
                if (assetUri is null)
                {
                    _eventLogService.LogWarning(
                        EventSource,
                        nameof(GetPageAttachmentBinaryAsync),
                        $"Attachment binary could not be resolved to a downloadable URI. PageGUID: {pageGuid}, AttachmentGUID: {attachmentGuid}, Channel: {channelName}, Culture: {culture}, WebsiteDomain: {websiteDomain ?? string.Empty}, AssetUrl: {asset.Url}.");
                    return null;
                }

                var response = await _httpClientFactory.CreateClient().GetAsync(assetUri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _eventLogService.LogWarning(
                        EventSource,
                        nameof(GetPageAttachmentBinaryAsync),
                        $"Attachment binary download failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase ?? string.Empty}. PageGUID: {pageGuid}, AttachmentGUID: {attachmentGuid}, Channel: {channelName}, Culture: {culture}, Uri: {assetUri}.");
                    return null;
                }

                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var metadata = asset.Metadata;
                var contentType = response.Content.Headers.ContentType?.MediaType
                    ?? GetMimeType(metadata?.Extension ?? string.Empty);
                var fileName = metadata?.Name ?? "attachment";

                return (data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _eventLogService.LogException(
                    EventSource,
                    nameof(GetPageAttachmentBinaryAsync),
                    ex,
                    $"Attachment binary retrieval failed. PageGUID: {pageGuid}, AttachmentGUID: {attachmentGuid}, Channel: {channelName}, Culture: {culture}.");
                throw;
            }
        }

        private static string GetMimeType(string extension)
        {
            if (ContentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
            {
                return contentType;
            }

            return MediaTypeNames.Application.Octet;
        }

        private static IEnumerable<ContentAttachment> GetAttachmentsFromPage(IWebPageFieldsSource page)
        {
            var properties = page.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (!typeof(IEnumerable<ContentAttachment>)
                        .IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                var value = property.GetValue(page) as IEnumerable<ContentAttachment>;

                if (value == null)
                {
                    continue;
                }

                foreach (var attachment in value)
                {
                    yield return attachment;
                }
            }
        }

    }
}
