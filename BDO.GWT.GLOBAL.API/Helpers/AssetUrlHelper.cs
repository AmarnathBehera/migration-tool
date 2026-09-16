using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.GLOBAL.API.Helpers;

internal static class AssetUrlHelper
{
    private const string DevelopmentLiveSiteHost = "localhost";
    private const int DevelopmentLiveSitePort = 12393;
    private const string VirtualContextAssetMarker = "/-/";

    public static Uri? BuildAssetUri(string assetUrl, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostEnvironment)
    {
        var trimmedAssetUrl = assetUrl.Trim();
        if (Uri.TryCreate(trimmedAssetUrl, UriKind.Absolute, out var absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return absoluteUri;
        }

        var normalizedPath = NormalizePath(trimmedAssetUrl);
        var queryIndex = normalizedPath.IndexOf('?');
        var path = queryIndex >= 0 ? normalizedPath[..queryIndex] : normalizedPath;
        var query = queryIndex >= 0 ? normalizedPath[(queryIndex + 1)..] : string.Empty;

        if (hostEnvironment.IsDevelopment())
        {
            return new UriBuilder(Uri.UriSchemeHttp, DevelopmentLiveSiteHost, DevelopmentLiveSitePort)
            {
                Path = path,
                Query = query
            }.Uri;
        }

        var request = httpContextAccessor.HttpContext?.Request;
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

    public static string NormalizePath(string assetUrl)
    {
        var normalizedPath = assetUrl.Trim();
        var markerIndex = normalizedPath.IndexOf(VirtualContextAssetMarker, StringComparison.Ordinal);
        if (markerIndex >= 0)
        {
            normalizedPath = normalizedPath[(markerIndex + VirtualContextAssetMarker.Length)..];
        }

        normalizedPath = normalizedPath.TrimStart('~');
        return normalizedPath.StartsWith('/') ? normalizedPath : $"/{normalizedPath}";
    }
}
