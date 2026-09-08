using BDO.GWT.Core.Components.Widget.TrendingTopicsCarouselWidget;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TrendingTopicsCarouselWidget;

public class TrendingTopicsCarouselWidget : ViewComponent
{
    private readonly ITrendingTopicsCarouselService _trendingTopicsCarouselService;

    public TrendingTopicsCarouselWidget(ITrendingTopicsCarouselService trendingTopicsCarouselService)
    {
        _trendingTopicsCarouselService = trendingTopicsCarouselService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<TrendingTopicsCarouselWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var pages = Enumerable.Empty<ContentItemReference>();

        if (model.Properties.TrendingTopicsPageType == "Industry")
        {
            pages = model.Properties.IndustryPages
                .Where(x => x.Identifier != Guid.Empty);
        }
        else if (model.Properties.TrendingTopicsPageType == "Service")
        {
            pages = model.Properties.ServicePages
                .Where(x => x.Identifier != Guid.Empty);
        }
        else if (model.Properties.TrendingTopicsPageType == "Microsite")
        {
            pages = model.Properties.MicrositePages
                .Where(x => x.Identifier != Guid.Empty);
        }
        else if (model.Properties.TrendingTopicsPageType == "Blog")
        {
            pages = model.Properties.Pages
                .Where(x => x.Identifier != Guid.Empty);
        }
        else if (model.Properties.TrendingTopicsPageType == "Specialty")
        {
            pages = model.Properties.SpecialtyPages
                .Where(x => x.Identifier != Guid.Empty);
        }
        else if (model.Properties.TrendingTopicsPageType == "Insight")
        {
            pages = model.Properties.InsightPages
                .Where(x => x.Identifier != Guid.Empty);
        }

        var data = new TrendingTopicsCarouselData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            TrendingTopicsPages = pages,
            LearnMoreButtonURLTarget = model.Properties.LearnMoreButtonURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };
        
        var dto = await _trendingTopicsCarouselService.GetTrendingTopicsCarouselAsync(data, cancellationToken);

        return View("~/Components/Widget/TrendingTopicsCarouselWidget/_TrendingTopicsCarouselWidget.cshtml", dto);
    }
}
