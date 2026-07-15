using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.GLOBAL.Components.Widget.InsightCarouselWidget;
using BDO.GWT.Web.Core.Data;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets;

public class InsightCarouselWidget : ViewComponent
{
    private readonly IInsightCarouselService _insightCarouselService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;

    public InsightCarouselWidget(
        IInsightCarouselService insightCarouselService,
       IContentService contentService,
       IContentRetriever contentRetriever)
    {
        _insightCarouselService = insightCarouselService;
        _contentService = contentService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<InsightCarouselWidgetProperties> model)
   {
        var page = model.Page?.WebPageItemGUID;

        if (page is null)
        {
            return View("~/Components/Widget/InsightCarouselWidget/_InsightCarouselWidget.cshtml", new InsightCarouselDto { FilteringTooltipEnabled = false });
        }
        var currentPage = (await _contentService.GetItemsByGUIDAsync(new List<Guid> { page.Value })).FirstOrDefault();

        var backgroundimageurl = model.Properties?.BackgroundImages?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImages.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
            : Enumerable.Empty<ContentAttachment>();

        var data = new InsightCarouselData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            Filters = model.Properties.Filters?.ToHashSet() ?? [],
            FeaturedInsightNodeGuids = model.Properties.FeaturedInsights ?? [],
            BackgroundImageURL = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _insightCarouselService.GetInsightCarouselAsync(currentPage, data, HttpContext.RequestAborted);

        return View("~/Components/Widget/InsightCarouselWidget/_InsightCarouselWidget.cshtml", dto);
    }
}
