using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Widgets.TabAccordionWidget;
using CMS.ContentEngine;
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
        var guids = new List<System.Guid>();


        if (model?.Properties?.PageType == "Industry" && model.Properties.IndustryPages.Any())
        {
            var accordionPages = new List<TabAccordionDto>();

             guids = model.Properties.IndustryPages
            .Where(x => x.Identifier != Guid.Empty)
            .Select(x => x.Identifier)
            .ToList();

            //if (guids == null || !guids.Any())
            //    return new List<TabAccordionDto>();

            var contentTypes = new[]
            {
                SectionIndustries.CONTENT_TYPE_NAME,
                IndustryCategory.CONTENT_TYPE_NAME,
                IndustryService.CONTENT_TYPE_NAME,
                Industry.CONTENT_TYPE_NAME,
                SectionServices.CONTENT_TYPE_NAME,
                ServiceArea.CONTENT_TYPE_NAME,
                Sub_Service.CONTENT_TYPE_NAME,
                BusinessLine.CONTENT_TYPE_NAME,
                Service.CONTENT_TYPE_NAME
            };

            var contentItems = await _contentRetriever
                .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                    contentTypes,
                    guids,
                    cancellationToken
                );

        }
        else if (model?.Properties?.PageType == "Service" && model.Properties.ServicePages.Any())
        {
            guids = model.Properties.ServicePages
                .Where(x => x.Identifier != Guid.Empty)
                .Select(x => x.Identifier)
                .ToList();

            
        }




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
        var contentTypes = new[]
            {
                SectionIndustries.CONTENT_TYPE_NAME,
                IndustryCategory.CONTENT_TYPE_NAME,
                IndustryService.CONTENT_TYPE_NAME,
                Industry.CONTENT_TYPE_NAME,
                SectionServices.CONTENT_TYPE_NAME,
                ServiceArea.CONTENT_TYPE_NAME,
                Sub_Service.CONTENT_TYPE_NAME,
                BusinessLine.CONTENT_TYPE_NAME,
                Service.CONTENT_TYPE_NAME
            };

        var contentItems = await _contentRetriever
            .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                contentTypes,
                pageGuids,
                cancellationToken
            );


        return contentItems.Select(p => new TabAccordionItemDto
        {
            Title = p.GetType()
                                .GetProperty("MetadataTitle")
                                ?.GetValue(p)
                                ?.ToString()
                            ?? p.SystemFields.ContentItemName,
            Content = p.GetType()
                                .GetProperty("MetadataDescription")
                                ?.GetValue(p)
                                ?.ToString()
                            ?? p.SystemFields.ContentItemName
        }).ToArray();
    }
}


