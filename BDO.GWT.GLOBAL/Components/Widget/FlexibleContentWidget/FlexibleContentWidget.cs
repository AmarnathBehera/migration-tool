using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services.Widget;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.FlexibleContentWidget;

public class FlexibleContentWidget : ViewComponent
{
    private readonly IFlexibleContentService _flexibleContentService;

    public FlexibleContentWidget(IFlexibleContentService flexibleContentService)
    {
        _flexibleContentService = flexibleContentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<FlexibleContentWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var data = new FlexibleContentData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FlexibleContentGuids = model.Properties.Pages.Select(x => x.Identifier),
            FeaturedFlexibleContentGuids = model.Properties.FeaturedPages.Select(x => x.Identifier),
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
            LearnMoreButtonURLTarget = model.Properties.LearnMoreButtonURLTarget,

        };

        var dto = await _flexibleContentService.GetFlexibleContentAsync(data, cancellationToken);

        return View("~/Components/Widget/FlexibleContentWidget/_FlexibleContentWidget.cshtml", dto);
    }
}
