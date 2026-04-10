

using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core;
public class ExternalNavigationData : BaseWidgetData
{
    public LinkItemDto Link { get; set; } = null!;
    public string openInNewTab { get; set; } = "_self";
}
