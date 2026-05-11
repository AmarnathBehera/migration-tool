using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class LocationsBannerDto
{
    public string Layout { get; set; } = "col-1";

    public string? Description { get; set; }

    public string? BackgroundImage { get; set; }

    public string? Theme { get; set; }

    public ButtonDto? PrimaryButton { get; set; }
}
