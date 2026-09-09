using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class ViewGlobalLocationsDto
{
    public string? Title { get; set; }
    
    public string? Description { get; set; }

    public string? BackgroundImage { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string Layout { get; set; } = null!;

    public ButtonDto Button { get; set; } = null!;

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }
}
