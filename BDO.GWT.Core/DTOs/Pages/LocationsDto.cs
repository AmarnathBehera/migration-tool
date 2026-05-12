using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs.Pages;

public class LocationsDto
{
    public HeroDto Hero { get; set; } = null!;

    public LocationsMainDto MainContent { get; set; } = null!;

    public LocationsBannerDto Banner { get; set; } = null!;
}
