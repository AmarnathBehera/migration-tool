using System;
using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public record SitemapUrlDto
{
    public required string LocationUri { get; init; }

    public DateTime? LastModificationDate { get; init; }

    public IEnumerable<SitemapAlternateUrlDto>? AlternateUrls { get; init; } = [];
}
