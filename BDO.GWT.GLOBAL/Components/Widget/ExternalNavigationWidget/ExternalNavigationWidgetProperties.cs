using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;


namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;

public class ExternalNavigationWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Order = 0, Label = "Link display text:")]
    public string LinkText { get; set; } = null!;

    [TextInputComponent(Order = 1, Label = "External Link:")]
    public string ExternalLink { get; set; } = null!;

    [DropDownComponent(Label = "Primary Button link target", Order = 2, Options = "_self;Same tab\r\n_blank;New tab")]
    public string PrimaryButtonLinkTarget { get; set; } = "_self";
}