using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.Data;

public class ViewGlobalLocationsData : BaseWidgetData
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string BackgroundImage { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string Layout { get; set; } = null!;
    public ButtonDto Button { get; set; } = null!;

    public string ButtonTitle { get; set; } = null!;

    public string ButtonURL { get; set; } = null!;

    public string ButtonType { get; set; } = null!;

    public string ButtonTarget { get; set; } = null!;

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

}
