using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.GLOBAL.Components.Widget.LocalNavigationWidget;


public class LocalNavigationWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Order = 0, Label = "Link display text")]
    public string LinkText { get; set; } = null!;

    [TextInputComponent(Order = 1, Label = "Widget ID to link to")]
    public string WidgetIdToLink { get; set; } = null!;
}
