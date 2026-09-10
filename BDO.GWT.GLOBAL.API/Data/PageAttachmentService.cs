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

            // For development, try to read from file system first for relative URLs
            if (_hostEnvironment.IsDevelopment() && IsRelativeUrl(asset.Url))
            {
                var fileData = await TryReadFileFromDiskAsync(asset.Url, cancellationToken);
                if (fileData is not null)
                {
                    var contentType = GetMimeType(asset.Metadata.Extension ?? string.Empty);
                    var fileName = asset.Metadata.Name ?? "attachment";
                    return (fileData, contentType, fileName);
                }
            }

            var response = await _httpClientFactory.CreateClient().GetAsync(assetUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var httpContentType = response.Content.Headers.ContentType?.MediaType
                ?? GetMimeType(asset.Metadata.Extension ?? string.Empty);
            var httpFileName = asset.Metadata.Name ?? "attachment";

            return (data, httpContentType, httpFileName);
        }

        private static string NormalizePath(string assetUrl)
        {
            var normalizedPath = assetUrl.Trim().TrimStart('~');
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
            if (Uri.TryCreate(assetUrl.Trim(), UriKind.Absolute, out var absoluteAssetUri))
            {
                return absoluteAssetUri;
            }

            var normalizedPath = NormalizePath(assetUrl);
            var queryIndex = normalizedPath.IndexOf('?');
            var path = queryIndex >= 0 ? normalizedPath[..queryIndex] : normalizedPath;
            var query = queryIndex >= 0 ? normalizedPath[(queryIndex + 1)..] : string.Empty;

            var request = _httpContextAccessor.HttpContext?.Request;

            if (_hostEnvironment.IsDevelopment())
            {
                // In development, always use fixed local live-site URL
                return new UriBuilder(Uri.UriSchemeHttp, DevelopmentLiveSiteHost, DevelopmentLiveSitePort)
                {
                    Path = path,
                    Query = query
                }.Uri;
            }

            // Outside development: use current request's host
            if (request is null)
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

        private bool IsRelativeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return url.StartsWith("/") || url.StartsWith("~") || !Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private async Task<byte[]?> TryReadFileFromDiskAsync(string assetUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var webRootPath = _hostEnvironment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    var contentRootPath = _hostEnvironment.ContentRootPath;
                    if (string.IsNullOrWhiteSpace(contentRootPath))
                    {
                        return null;
                    }

                    webRootPath = System.IO.Path.Combine(contentRootPath, "wwwroot");
                }

                var normalizedPath = NormalizePath(assetUrl);
                var fullPath = System.IO.Path.Combine(webRootPath, normalizedPath.TrimStart('/'));

                // Security check: ensure the path doesn't escape the web root
                var fullWebRootPath = System.IO.Path.GetFullPath(webRootPath);
                var fullAssetPath = System.IO.Path.GetFullPath(fullPath);
                if (!fullAssetPath.StartsWith(fullWebRootPath, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (File.Exists(fullAssetPath))
                {
                    return await File.ReadAllBytesAsync(fullAssetPath, cancellationToken);
                }

                return null;
            }
            catch
            {
                // If anything goes wrong reading from disk, return null to fall back to HTTP
                return null;
            }
        }
    }
}
