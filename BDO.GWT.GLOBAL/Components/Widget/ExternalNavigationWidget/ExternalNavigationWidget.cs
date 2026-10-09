using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
public class ExternalNavigationWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<ExternalNavigationWidgetProperties> model)
    {
       
        var dto = new ExternalNavigationData
        {
            Link = new LinkItemDto
            {
                Title = model.Properties.LinkText,
                URL = model.Properties.ExternalLink
            },
            openInNewTab = model.Properties.PrimaryButtonLinkTarget
        };
        return View("~/Components/Widget/ExternalNavigationWidget/_ExternalNavigationWidget.cshtml", dto);
    }
}

