
using BDO;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[assembly: RegisterWidget(
    ComponentIdentifiers.Globallocation,
    typeof(ViewGlobalLocationsWidget),
    "View Global Location",
    typeof(ViewGlobalLocationsWidgetProperties),
    AllowCache = true,
    IconClass = "icon-xml-tag"
)]
namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;

public class ViewGlobalLocationsWidget : ViewComponent
{
   
    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<ViewGlobalLocationsWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var data = new ViewGlobalLocationsData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            Theme = model.Properties.Theme,
            Layout = model.Properties.Layout,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            ButtonTitle = model.Properties.ButtonText,
            ButtonURL = model.Properties.ButtonUrl,
            ButtonTarget = model.Properties.ButtonLinkTarget,
        };

        return View("~/Components/Widget/ViewGlobalLocations/_ViewGlobalLocations.cshtml", data);
    }

  
}
