using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;



public class SitemapPageDto
{
    public HeroDto Hero { get; set; } = null!;

    public List<SitemapItemDto> Items { get; set; } = new();
}

