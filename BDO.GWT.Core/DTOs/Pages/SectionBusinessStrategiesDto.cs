using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages;

public class SectionBusinessStrategiesDto
{
    public HeroDto Hero { get; set; } = null!;

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public List<BusinessStrategiesMenuDto> BusinessStrategiesMenu { get; set; } = new();
}
