using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.Data;

public class ViewGlobalLocationsData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? BackgroundImage { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string Layout { get; set; } = null!;

    public string? ButtonTitle { get; set; }

    public string? ButtonURL { get; set; }

    public string? ButtonType { get; set; }

    public string? ButtonTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
