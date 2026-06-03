using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Web.Core.DTOs.Pages;

public class DealDto
{
    public HeroDto Hero { get; set; } = null!;

    public string? Title { get; set; }

    public List<DealCardDto> DealTypeNames { get; set; } = [];

    public List<DealCardDto> IndustryNames { get; set; } = [];

    public List<DealCardDto> SubIndustryNames { get; set; } = [];

    public List<DealCardDto> Range { get; set; } = [];

    public string? ClientName { get; set; }

    public string? Content { get; set; }

    public DealSidebarDto? Sidebar { get; set; }
}
