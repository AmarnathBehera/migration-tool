using BDO.GWT.Core.DTOs;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.HtmlWidget;

public class HtmlWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<HtmlWidgetProperties> model)
    {
        var dto = new HtmlDto
        {
            HtmlCode = model.Properties.HtmlCode,
        };

        return View("~/Components/Widget/HtmlWidget/_HtmlWidget.cshtml", dto);
    }
}
