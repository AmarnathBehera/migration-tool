using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.LiveSite.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets.NewsCarouselWidget;

public class NewsCarouselWidget : ViewComponent
{
    private readonly INewsCarouselService _newsCarouselService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;

    public NewsCarouselWidget(
        INewsCarouselService newsCarouselService, 
       IContentService contentService,
       IContentRetriever contentRetriever)
    {
        _newsCarouselService = newsCarouselService;
        _contentService = contentService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<NewsCarouselWidgetProperties> model)
    {
        if (model is null)
        {
            return View("~/Components/Widget/NewsCarouselWidget/_NewsCarouselWidget.cshtml", new NewsCarouselDto { FilteringTooltipEnabled = false });
        }

        var page = model?.Page?.WebPageItemGUID;
        var currentPage = (await _contentService.GetItemsByGUIDAsync(new List<Guid> { page.Value })).FirstOrDefault();

        var backgroundimageurl = model.Properties?.BackgroundImages?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImages.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
            : Enumerable.Empty<ContentAttachment>();

        var data = new NewsCarouselData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            Filters = model.Properties.Filters?.ToHashSet() ?? [],
            NodeGuidsOfFeaturedNews = model.Properties.FeaturedNews ?? [],
            BackgroundImageURL = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _newsCarouselService.GetNewsCarouselAsync(currentPage, data, HttpContext.RequestAborted);

        return View("~/Components/Widget/NewsCarouselWidget/_NewsCarouselWidget.cshtml",dto);
    }
}
