using BDO.GWT.Core;
using BDO.GWT.Core.Components.Widgets.RelatedContentCarouselWidget;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Core.LiveSite.Data;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.RelatedContentCarouselWidget;

public class RelatedContentCarouselWidget : ViewComponent
{
    private readonly IRelatedContentCarouselService _relatedContentCarouselService;
    private readonly IContentRetriever _contentRetriever;

    public RelatedContentCarouselWidget(IRelatedContentCarouselService relatedContentCarouselService, IContentRetriever contentRetriever)
    {
        _relatedContentCarouselService = relatedContentCarouselService;
        _contentRetriever = contentRetriever;


    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<RelatedContentCarouselWidgetProperties> model)
    {
        var page = model.Page?.WebPageItemGUID;

        if (page is null)
        {
            return View("~/Components/Widget/RelatedContentCarouselWidget/_RelatedContentCarouselWidget.cshtml", new RelatedContentCarouselDto { FilteringTooltipEnabled = false });
        }


        var backgroundimageurl = model.Properties?.BackgroundImages?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImages.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 2 })
            : Enumerable.Empty<ContentAttachment>();

        var selectedPageTypes = model.Properties.SelectedPageTypes?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() is { Count: > 0 } types
            ? types
            : GetDefaultPageTypes();
        var filters = model.Properties.Filters?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? GetDefaultFilters();

        var data = new RelatedContentCarouselData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            SelectedPageTypes = selectedPageTypes,
            Filters = filters.ToHashSet(),
            FeaturedPageNodeGuids = model.Properties.FeaturedPages ?? [],
            BackgroundImageUrl = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
            
            
        };

        var dto = await _relatedContentCarouselService.GetRelatedContentCarouselDtoAsync(page.Value, model.Page.ContentTypeName, data, HttpContext.RequestAborted);

        return View("~/Components/Widget/RelatedContentCarouselWidget/_RelatedContentCarouselWidget.cshtml", dto);
    }
    private static List<string> GetDefaultPageTypes()
    {
        return new List<string>
        {
            nameof(BlogPost),
            nameof(Event),
            nameof(Insight),
            nameof(MicrositePage),
            nameof(NewsArticle),
            nameof(Webinar)
        };
    }

    private static List<string> GetDefaultFilters()
    {
        return new List<string>
        {
            FilterParameterName.ContentType,
            FilterParameterName.BusinessLine,
            FilterParameterName.IndustryCategory
        };
    }
}
