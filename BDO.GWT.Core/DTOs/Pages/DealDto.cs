using BDO.GWT.Web.Core.DTOs;

public class DealDto
{
    public HeroDto Hero { get; set; } = null!;

    public string? Title { get; set; } = null!;

    public List<string> DealTypeNames { get; set; } = new();

    public List<string> IndustryNames { get; set; } = new();

    public List<string> SubIndustryNames { get; set; } = new();

    public string? ClientName { get; set; }

    public string? Content { get; set; }

    public DealSidebarDto? Sidebar { get; set; }
}
