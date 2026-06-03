using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Widgets.TabAccordionWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Components.Widget.TabAccordionWidget;

public class TabAccordionWidget : ViewComponent
{
    private readonly ITabAccordionService _tabAccordionService;

    public TabAccordionWidget(
        ITabAccordionService tabAccordionService)
    {
        _tabAccordionService = tabAccordionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        ComponentViewModel<TabAccordionWidgetProperties> model)
    {
        var guids = Enumerable.Empty<Guid>();

        if (model.Properties.PageType == "Industry")
        {
            guids = model.Properties.IndustryPages
                .Where(x => x.Identifier != Guid.Empty)
                .Select(x => x.Identifier);
        }
        else if (model.Properties.PageType == "Service")
        {
            guids = model.Properties.ServicePages
                .Where(x => x.Identifier != Guid.Empty)
                .Select(x => x.Identifier);
        }

        var data = new TabAccordionData
        {
            Title = model.Properties.Title,
            Layout = model.Properties.Layout,
            PageGuids = guids,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID
        };

        var dto = await _tabAccordionService.GetTabAccordionAsync(
            data,
            HttpContext.RequestAborted);

        return View(
            "~/Components/Widget/TabAccordionWidget/_TabAccordionWidget.cshtml",
            dto);
    }
}