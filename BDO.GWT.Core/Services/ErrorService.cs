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
        string languageName,
        string channelName,
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
        string languageName,
        string channelName,
        CancellationToken cancellationToken)
    {
        if (statusCodeReExecuteFeature is null)
        {
            return null;
        }

        var statusCodePage = await RetrieveStatusCodePageAsync(
            statusCodeReExecuteFeature.OriginalStatusCode,
            languageName,
            channelName,
            cancellationToken);

        if (statusCodePage is null)
        {
            return null;
        }

        return statusCodePage;
    }

    private async Task<StatusCode?> RetrieveStatusCodePageAsync(
    int originalStatusCode,
    string languageName,
    string channelName,
    CancellationToken cancellationToken)
    {
        var statusCodePage = (await _contentRetriever
            .GetPagesAsync<StatusCode>(
            new RetrievePagesParameters { 
                LanguageName = languageName,
                ChannelName = channelName,
                IsForPreview=false
            }
                ,
                query =>
                {
                    query.Where(w => w.WhereEquals(nameof(StatusCode.HttpStatusCode), originalStatusCode));
                }, RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            ))
            .FirstOrDefault();

        return statusCodePage;
    }
}