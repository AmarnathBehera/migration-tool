

namespace BDO.GWT.Web.Core.DTOs;


public class CareerPageDto
{
    public HeroDto Hero { get; set; } = null!;

    public CareerContentDto Content { get; set; } = null!;

    public string? Form { get; set; }

    public ContentSidebarDto? Sidebar { get; set; }
}
