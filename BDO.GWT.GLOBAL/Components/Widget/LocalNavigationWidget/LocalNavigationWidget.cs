
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;

using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.Components.Widget.LocalNavigationWidget;

public class LocalNavigationWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<LocalNavigationWidgetProperties> model)
    {
        var dto = new LocalNavigationData
        {
            Link = new LinkItemDto
            {
                Title = model.Properties.LinkText,
                URL = $"#{model.Properties.WidgetIdToLink}"
            }
        };

        return View("~/Components/Widget/LocalNavigationWidget/_LocalNavigationWidget.cshtml", dto);
    }
}
