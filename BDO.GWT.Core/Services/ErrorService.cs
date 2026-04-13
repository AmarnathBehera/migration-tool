using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface IErrorService
{
    Task<StatusCodePageDto?> GetStatusCodePageDtoAsync(
        IStatusCodeReExecuteFeature? statusCodeReExecuteFeature,
        CancellationToken cancellationToken);
}

public class ErrorService : IErrorService
{
    private readonly IContentService _contentRetriever;
    private readonly IStatusCodePageService _statusCodePageService;
    private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;


    public ErrorService(
        IContentService contentRetriever,
        IStatusCodePageService statusCodePageService,
        IWebPageDataContextInitializer webPageDataContextInitializer)
    {
        _contentRetriever = contentRetriever;
        _statusCodePageService = statusCodePageService;
        _webPageDataContextInitializer = webPageDataContextInitializer;
    }

    public async Task<StatusCodePageDto?> GetStatusCodePageDtoAsync(
        IStatusCodeReExecuteFeature? statusCodeReExecuteFeature,
        CancellationToken cancellationToken)
    {
        if (statusCodeReExecuteFeature is null)
        {
            return null;
        }

        var statusCodePage = await RetrieveStatusCodePageAsync(
            statusCodeReExecuteFeature.OriginalStatusCode,
            cancellationToken);

        if (statusCodePage is null)
        {
            return null;
        }

        //_webPageDataContextInitializer.Initialize(statusCodePage);

        var dto = _statusCodePageService.GetStatusCodePageDto(statusCodePage);

        return dto;
    }

    private async Task<StatusCode?> RetrieveStatusCodePageAsync(
    int originalStatusCode,
    CancellationToken cancellationToken)
    {
        var statusCodePage = (await _contentRetriever
            .GetPagesAsync<StatusCode>(
            new RetrievePagesParameters { }
                ,
                query =>
                {
                    query.Where(w => w.WhereEquals("StatusCode", originalStatusCode));
                },RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            ))
            .FirstOrDefault();

        return statusCodePage;
    }
}