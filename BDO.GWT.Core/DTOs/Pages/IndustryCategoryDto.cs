using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class IndustryCategoryDto
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public HeroDto Hero { get; set; } = null!;

    public SideBoxDto? Sidebox { get; set; }
}
