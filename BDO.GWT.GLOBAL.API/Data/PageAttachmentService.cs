using BDO.GWT.Core;
using CMS.Websites;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Data
{
    public class PageAttachmentService : IPageAttachmentService
    {
        private const string DevelopmentLiveSiteHost = "localhost";
        private const int DevelopmentLiveSitePort = 12393;

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

        private static string NormalizePath(string assetUrl)
        {
            const string virtualContextAssetMarker = "/-/";

            var normalizedPath = assetUrl.Trim();
            var markerIndex = normalizedPath.IndexOf(virtualContextAssetMarker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                normalizedPath = normalizedPath[(markerIndex + virtualContextAssetMarker.Length)..];
            }

            normalizedPath = normalizedPath.TrimStart('~');
            return normalizedPath.StartsWith('/') ? normalizedPath : $"/{normalizedPath}";
        }

        private static string GetMimeType(string extension)
        {
            if (ContentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
            {
                return contentType;
            }

            return MediaTypeNames.Application.Octet;
        }

        private Uri? BuildAssetUri(string assetUrl)
        {
            var trimmedAssetUrl = assetUrl.Trim();
            if (Uri.TryCreate(trimmedAssetUrl, UriKind.Absolute, out var absoluteAssetUri)
                && (absoluteAssetUri.Scheme == Uri.UriSchemeHttp || absoluteAssetUri.Scheme == Uri.UriSchemeHttps))
            {
                return absoluteAssetUri;
            }

            var normalizedPath = NormalizePath(trimmedAssetUrl);
            var queryIndex = normalizedPath.IndexOf('?');
            var path = queryIndex >= 0 ? normalizedPath[..queryIndex] : normalizedPath;
            var query = queryIndex >= 0 ? normalizedPath[(queryIndex + 1)..] : string.Empty;

            if (_hostEnvironment.IsDevelopment())
            {
                return new UriBuilder(Uri.UriSchemeHttp, DevelopmentLiveSiteHost, DevelopmentLiveSitePort)
                {
                    Path = path,
                    Query = query
                }.Uri;
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is null
                || string.IsNullOrWhiteSpace(request.Scheme)
                || !request.Host.HasValue
                || string.IsNullOrWhiteSpace(request.Host.Host))
            {
                return null;
            }

            return new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? -1)
            {
                Path = path,
                Query = query
            }.Uri;
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
