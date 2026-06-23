namespace BDO.GWT.Core.DTOs;

public record SitemapAlternateUrlDto
{
    public required string Url { get; init; }

    public required string LanguageTag { get; init; }
}
