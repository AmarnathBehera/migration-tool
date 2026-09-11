using BDO.GWT.Web.Core.LiveSite.Data;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using static BDO.ComponentIdentifiers;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ButtonWidget;

public class ButtonWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<ButtonWidgetProperties> model)
    {
        var dto = new ButtonData
        {
            Text = model.Properties.Text,
            Link = model.Properties.Link,
            Target = model.Properties.Target,
            Type = model.Properties.Type,
            Theme = model.Properties.Theme,
            ButtonSide = model.Properties.ButtonSide,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };
        return View("~/Components/Widget/ButtonWidget/_ButtonWidget.cshtml", dto);
    }
}
