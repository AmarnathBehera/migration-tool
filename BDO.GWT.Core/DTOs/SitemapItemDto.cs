

namespace BDO.GWT.Core.DTOs;
public class SitemapItemDto
{
    public string Title { get; set; } = null!;

    public string? Link { get; set; }

    public List<SitemapItemDto> Items { get; set; } = new();
}
