using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Core.Services;

public class LocalizedUrl
{
    public required string LanguageName { get; init; }
    public required string CultureFormat { get; init; }
    public required string DisplayName { get; init; }
    public required string ShortCode { get; init; }
    public required string RelativeUrl { get; init; }
    public required string AbsoluteUrl { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}

public interface ILocalizedUrlsBuilder
{
    Task<IReadOnlyList<LocalizedUrl>> BuildAsync(HttpContext httpContext);
}
