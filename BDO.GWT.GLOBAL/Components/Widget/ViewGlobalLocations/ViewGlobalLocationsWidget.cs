using AngleSharp.Dom;
using BDO;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            Button = new ButtonDto
            {
                Title = model.Properties.ButtonText,
                URL = model.Properties.ButtonUrl,
                Target = model.Properties.ButtonUrl,
                Type = "secondary",
            },
            BackgroundImage= "~/images/globalviewimage.jpg"
        };


        

        return View("~/Components/Widget/ViewGlobalLocations/_ViewGlobalLocations.cshtml", data);
    }

  
}
