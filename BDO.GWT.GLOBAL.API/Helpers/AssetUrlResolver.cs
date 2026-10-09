using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.GLOBAL.API.Helpers;

public interface IAssetUrlResolver
{
    Uri? BuildAssetUri(string assetUrl, string? websiteDomain = null);
}

public sealed class AssetUrlResolver(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment hostEnvironment)
    : IAssetUrlResolver
{
    public Uri? BuildAssetUri(string assetUrl, string? websiteDomain = null) =>
        AssetUrlHelper.BuildAssetUri(assetUrl, httpContextAccessor, hostEnvironment, websiteDomain);
}
