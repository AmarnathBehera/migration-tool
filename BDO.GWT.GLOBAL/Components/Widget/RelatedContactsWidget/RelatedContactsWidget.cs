using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Widgets.RelatedContactsWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets;

public class RelatedContactsWidget : ViewComponent
{
    private readonly IRelatedContactsService _relatedContactsService;
    private readonly IContentService _contentService;

    public RelatedContactsWidget(IRelatedContactsService relatedContactsService, IContentService contentService)
    {
        _relatedContactsService = relatedContactsService;
        _contentService = contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<RelatedContactsWidgetProperties> model)
    {
        var page = model.Page?.WebPageItemGUID;
        if (page is null)
        {
            return View("~/Components/Widget/RelatedContactsWidget/_RelatedContactsWidget.cshtml", new PeopleGridDto());
        }

        var currentPage = (await _contentService.GetItemsByGUIDAsync(new List<Guid> { page.Value })).FirstOrDefault();
        if (currentPage is null)
        {
            return View("~/Components/Widget/RelatedContactsWidget/_RelatedContactsWidget.cshtml", new PeopleGridDto());
        }

        var data = new RelatedContactsData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            OrderCriteria = model.Properties.OrderCriteria,
            IsDescendingOrder = model.Properties.IsDescendingOrder,
            ShowOffice = model.Properties.ShowOffice,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _relatedContactsService.GetRelatedContactsDtoAsync(currentPage, data, HttpContext.RequestAborted);
        return View("~/Components/Widget/RelatedContactsWidget/_RelatedContactsWidget.cshtml", dto);
    }
}