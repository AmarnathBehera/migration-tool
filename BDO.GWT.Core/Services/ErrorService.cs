using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using CMS.Core;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace BDO.GWT.Core.Services;

public interface IErrorService
{
    Task<StatusCode?> GetStatusCodePageAsync(
        IStatusCodeReExecuteFeature? statusCodeReExecuteFeature,
        CancellationToken cancellationToken);
}

public class ErrorService : IErrorService
{
    private readonly IContentService _contentRetriever;



    public ErrorService(
        IContentService contentRetriever
)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<StatusCode?> GetStatusCodePageAsync(
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

        return statusCodePage;
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