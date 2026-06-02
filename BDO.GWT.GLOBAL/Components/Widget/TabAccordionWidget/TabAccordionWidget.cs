using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Widgets.TabAccordionWidget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Components.Widget.TabAccordionWidget;

public class TabAccordionWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;


    public TabAccordionWidget(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<TabAccordionWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        List<System.Guid> guids = model.Properties.PageType switch
        {
            "Service" => model.Properties.ServicePages.Select(p => p.WebPageGuid).ToList(),
            "Industry" => model.Properties.IndustryPages.Select(p => p.WebPageGuid).ToList(),
            "Microsite" => model.Properties.MicrositePages.Select(p => p.WebPageGuid).ToList(),
            _ => new List<System.Guid>()
        };

        var data = new TabAccordionData
        {
            Title = model.Properties.Title,
            Layout = model.Properties.Layout,
            PageGuids = guids,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await GetTabAccordionAsync(data, cancellationToken);

        return View("~/Components/Widget/TabAccordionWidget/_TabAccordionWidget.cshtml", dto);
    }

    public async Task<TabAccordionDto> GetTabAccordionAsync(TabAccordionData data, CancellationToken cancellationToken)
    {
        var itemDtos = await GetTabAccordionItemDtosAsync(data.PageGuids, cancellationToken);

        return new TabAccordionDto
        {
            Title = data.Title,
            Items = itemDtos,
            Layout = data.Layout,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };
    }

    private async Task<TabAccordionItemDto[]> GetTabAccordionItemDtosAsync(IEnumerable<Guid> pageGuids, CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(
            
            pageGuids,cancellationToken);

        return pages.Select(p => new TabAccordionItemDto
        {
            Title = p.SystemFields.ContentItemName,
            Content = p.SystemFields.WebPageItemTreePath
        }).ToArray();
    }
}


