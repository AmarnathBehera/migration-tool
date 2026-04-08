using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;

public class ExternalNavigationWidgetProperties : IWidgetProperties
{
    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 0, Label = "Link display text:")]
    public string LinkText { get; set; } = null!;

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 1, Label = "External Link:")]
    public string ExternalLink { get; set; } = null!;

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Primary Button link target", Order = 4)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "_self;Same tab\r\n_blank;New tab")]
    public string PrimaryButtonLinkTarget { get; set; } = "_self";
}