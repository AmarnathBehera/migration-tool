using BDO.GWT.Core.Entities;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services.Widget;
public class ContentRetrieverWrapper : IContentRetrieverWrapper
{
    private readonly IContentRetriever _contentRetriever;

    public ContentRetrieverWrapper(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<IEnumerable<IContentItemFieldsSource>> RetrieveButtonUrlAsync(
        IEnumerable<Guid> guids,
        string[] contentTypes)
    {
        return await _contentRetriever
            .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                contentTypes,
                guids.ToList(),
                new RetrieveContentOfContentTypesParameters
                {
                    LinkedItemsMaxLevel = 3
                });
    }
}