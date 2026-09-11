using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EventsWebinarsSliderWidget;

public class EventsWebinarsSliderWidget : ViewComponent
{
    private readonly IEventsWebinarsSliderService _eventsWebinarsSliderService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;

    public EventsWebinarsSliderWidget(
        IEventsWebinarsSliderService eventsWebinarsSliderService,
        IContentRetriever contentRetriever, IContentService contentService)
    {
        _eventsWebinarsSliderService = eventsWebinarsSliderService;
        _contentRetriever = contentRetriever;
        _contentService= contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        ComponentViewModel<EventsWebinarsSliderWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

       
        var backgroundimageurl = model.Properties.BackgroundImages?.Any() == true
         ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
             model.Properties.BackgroundImages.Select(i => i.Identifier).ToList(),
             new RetrieveContentParameters { LinkedItemsMaxLevel = 1 })
         : Enumerable.Empty<ContentAttachment>();
        var data = new EventsWebinarsSliderData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            Filters = model.Properties.Filters?.ToHashSet() ?? new HashSet<string>(),
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText)
                ? null
                : model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            FeaturedEventAndWebinarNodeGuids = model.Properties.FeaturedEventsAndWebinars,
            BackgroundImages = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            WidgetID = model.Properties.WidgetID,
        };
        var page = model.Page?.WebPageItemGUID;

        if (page is null)
        {
            return View("~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml", new EventsWebinarsSliderDto { FilteringTooltipEnabled = false });
        }
        var currentPage = (await _contentService.GetItemsByGUIDAsync(new List<Guid> { page.Value })).FirstOrDefault();

        var dto = await _eventsWebinarsSliderService.GetEventsWebinarsSliderAsync(
            currentPage, data, cancellationToken);

        return View(
            "~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml",
            dto);
    }
}