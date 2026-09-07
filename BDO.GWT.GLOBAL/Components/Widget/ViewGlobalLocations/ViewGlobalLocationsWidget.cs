using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Core.DTOs;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets;

public class ViewGlobalLocationsWidget : ViewComponent
{    private readonly IViewGlobalLocationsService _viewGlobalLocationsService;

    public ViewGlobalLocationsWidget(IViewGlobalLocationsService viewGlobalLocationsService)
    {
        _viewGlobalLocationsService = viewGlobalLocationsService;
    }
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
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _viewGlobalLocationsService.GetGlobalLocationsBannerAsync(data, cancellationToken);

        return View("~/Components/Widget/ViewGlobalLocations/_ViewGlobalLocations.cshtml", dto);
    }

  
}
