using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs;

public record SitemapDto
{
    public required IEnumerable<SitemapUrlDto> Urls { get; init; }
}
