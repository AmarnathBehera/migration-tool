using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.Websites;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Data
{
    public class PageAttachmentService : IPageAttachmentService
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        private readonly IContentService _contentService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostEnvironment;
        public PageAttachmentService(
            IContentService contentService,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostEnvironment)
        {
            _contentService = contentService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _hostEnvironment = hostEnvironment;
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
            var attachments = await GetPageAttachmentsAsync(pageGuid, channelName, culture, cancellationToken);
            var attachment = attachments.FirstOrDefault(x => x.SystemFields.ContentItemGUID == attachmentGuid);
            var asset = attachment?.ContentAttachmentAsset;

            if (asset?.Url is null)
            {
                return null;
            }

            var assetUri = BuildAssetUri(asset.Url);
            if (assetUri is null)
            {
                return null;
            }

            var response = await _httpClientFactory.CreateClient().GetAsync(assetUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType
                ?? GetMimeType(asset.Metadata.Extension ?? string.Empty);
            var fileName = asset.Metadata.Name ?? "attachment";

            return (data, contentType, fileName);
        }

        private static string GetMimeType(string extension)
        {
            if (ContentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
            {
                return contentType;
            }

            return MediaTypeNames.Application.Octet;
        }

        private Uri? BuildAssetUri(string assetUrl) =>
            AssetUrlHelper.BuildAssetUri(assetUrl, _httpContextAccessor, _hostEnvironment);

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