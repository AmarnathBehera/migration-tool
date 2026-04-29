using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Widgets.RichTextWidget;

public class RichTextWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<RichTextWidgetProperties> model)
    {
        return View("~/Components/Widget/RichTextWidget/_RichTextWidget.cshtml", model);
    }
}