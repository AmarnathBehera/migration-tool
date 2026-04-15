using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.RichTextWidget;

public class RichTextWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<RichTextWidgetProperties> model)
    {
        return View("~/Components/Widget/RichTextWidget/_RichTextWidget.cshtml", model);
    }
}