using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EditorsPickWidget;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Components.Widgets.EditorsPickWidget;

public class EditorsPickWidget : ViewComponent
{
    private readonly IEditorsPickService _editorsPickService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;

    public EditorsPickWidget(
        IEditorsPickService editorsPickService,
        IContentRetriever contentRetriever,
        IContentService contentService)
    {
        _editorsPickService = editorsPickService;
        _contentRetriever = contentRetriever;
        _contentService = contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<EditorsPickWidgetProperties> model)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 2 },
            HttpContext.RequestAborted);

        if (currentPage is null)
        {
            return View("~/Components/Widget/EditorsPickWidget/_EditorsPickWidget.cshtml", new EditorsPickDto
            {
                FilteringTooltipEnabled = false,
                Items = []
            });
        }

        var backgroundImageUrl = model.Properties.BackgroundImages?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImages.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 1 })
            : Enumerable.Empty<ContentAttachment>();

        var data = new EditorsPickData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            Filters = model.Properties.Filters?.ToHashSet() ?? new HashSet<string>(),
            EditorsPickGuids = model.Properties.Pages ?? [],
            FeaturedPageGuids = model.Properties.FeaturedPages ?? [],
            BackgroundImageUrl = backgroundImageUrl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText)
                ? null
                : model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _editorsPickService.GetEditorsPickAsync(currentPage, data, HttpContext.RequestAborted);

        return View("~/Components/Widget/EditorsPickWidget/_EditorsPickWidget.cshtml", dto);
    }
}