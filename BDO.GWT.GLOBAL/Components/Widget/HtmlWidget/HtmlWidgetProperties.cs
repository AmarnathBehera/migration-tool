
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.HtmlWidget;

public class HtmlWidgetProperties : IWidgetProperties
{
    [TextAreaComponent( Label = "HTML code", Order = 0)]
    public string HtmlCode { get; set; } = null!;
}
