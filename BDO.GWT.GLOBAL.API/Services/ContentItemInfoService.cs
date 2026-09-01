using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Implementation of IContentItemInfoService that wraps IInfoProvider extension methods.
/// </summary>
public class ContentItemInfoService : IContentItemInfoService
{
    private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;

    public ContentItemInfoService(IInfoProvider<ContentItemInfo> contentItemInfoProvider)
    {
        _contentItemInfoProvider = contentItemInfoProvider;
    }

    public async Task<ContentItemInfo?> GetByIdAsync(int contentItemId, CancellationToken cancellationToken = default)
    {
        return await _contentItemInfoProvider.GetAsync(contentItemId, cancellationToken);
    }
}
