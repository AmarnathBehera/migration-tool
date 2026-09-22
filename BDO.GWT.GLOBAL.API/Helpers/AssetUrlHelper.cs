using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.GLOBAL.API.Helpers;

internal static class AssetUrlHelper
{
    private const string DevelopmentLiveSiteHost = "localhost";
    private const int DevelopmentLiveSitePort = 12393;
    private const string VirtualContextAssetMarker = "/-/";

    public static Uri? BuildAssetUri(
        string assetUrl,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment,
        string? websiteDomain = null)
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

        var baseUri = ResolveBaseUri(websiteDomain, httpContextAccessor, hostEnvironment);
        if (baseUri is not null)
        {
            return new UriBuilder(baseUri)
            {
                Path = path,
                Query = query
            }.Uri;
        }

        return null;
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

    private static Uri? ResolveBaseUri(
        string? websiteDomain,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment)
    {
        if (!string.IsNullOrWhiteSpace(websiteDomain))
        {
            var trimmedWebsiteDomain = websiteDomain.Trim();
            if (Uri.TryCreate(trimmedWebsiteDomain, UriKind.Absolute, out var absoluteBaseUri)
                && (absoluteBaseUri.Scheme == Uri.UriSchemeHttp || absoluteBaseUri.Scheme == Uri.UriSchemeHttps))
            {
                return absoluteBaseUri;
            }

            var requestScheme = httpContextAccessor.HttpContext?.Request.Scheme;
            var scheme = string.IsNullOrWhiteSpace(requestScheme)
                ? Uri.UriSchemeHttps
                : requestScheme;

            if (Uri.TryCreate($"{scheme}://{trimmedWebsiteDomain}", UriKind.Absolute, out var normalizedBaseUri))
            {
                return normalizedBaseUri;
            }
        }

        if (hostEnvironment.IsDevelopment() || UseLocalLiveSiteHost(httpContextAccessor))
        {
            return new UriBuilder(Uri.UriSchemeHttp, DevelopmentLiveSiteHost, DevelopmentLiveSitePort).Uri;
        }

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null
            || string.IsNullOrWhiteSpace(request.Scheme)
            || !request.Host.HasValue
            || string.IsNullOrWhiteSpace(request.Host.Host))
        {
            return null;
        }

        return new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? -1).Uri;
    }

    private static bool UseLocalLiveSiteHost(IHttpContextAccessor httpContextAccessor)
    {
        var host = httpContextAccessor.HttpContext?.Request.Host.Host;
        return string.Equals(host, DevelopmentLiveSiteHost, StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }
}
