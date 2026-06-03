using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface ITabAccordionService
{
    Task<TabAccordionDto> GetTabAccordionAsync(
        TabAccordionData data,
        CancellationToken cancellationToken = default);
}


public class TabAccordionService : ITabAccordionService
{
    private readonly IContentRetriever _contentRetriever;

    public TabAccordionService(
        IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<TabAccordionDto> GetTabAccordionAsync(
        TabAccordionData data,
        CancellationToken cancellationToken = default)
    {
        var items = await GetTabAccordionItemsAsync(
            data.PageGuids,
            cancellationToken);

        return new TabAccordionDto
        {
            Title = data.Title,
            Layout = data.Layout,
            Items = items,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID
        };
    }

    private async Task<TabAccordionItemDto[]> GetTabAccordionItemsAsync(
        IEnumerable<Guid> pageGuids,
        CancellationToken cancellationToken)
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

        var contentItems =
            await _contentRetriever
                .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                    contentTypes,
                    pageGuids,
                    cancellationToken);

        return contentItems
            .Select(x => new TabAccordionItemDto
            {
                Title =
                    x.GetType()
                        .GetProperty("MetadataTitle")
                        ?.GetValue(x)
                        ?.ToString()
                    ?? x.SystemFields.ContentItemName,

                Content =
                    x.GetType()
                        .GetProperty("MetadataDescription")
                        ?.GetValue(x)
                        ?.ToString()
                    ?? x.SystemFields.ContentItemName
            })
            .ToArray();
    }
}