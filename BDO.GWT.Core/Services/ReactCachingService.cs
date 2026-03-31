using BDO.GWT.Common.Options;
using BDO.GWT.Web.Services;
using CMS.Helpers;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.Core.Services;

public class ReactCachingService : IReactCachingService
{
    private readonly IOptions<ReactCachingOptions> _reactCachingOptions;
    private readonly IProgressiveCache _progressiveCache;

    public ReactCachingService(IOptions<ReactCachingOptions> reactCachingOptions, IProgressiveCache progressiveCache)
    {
        _reactCachingOptions = reactCachingOptions;
        _progressiveCache = progressiveCache;
    }

    public async Task<TData> LoadAsync<TData>(Func<CancellationToken, Task<TData>> loadAsync, string cacheKey, CancellationToken cancellationToken)
    {
        if (!_reactCachingOptions.Value.CachingEnabled)
        {
            return await loadAsync(cancellationToken);
        }

        var cacheSettings = new CacheSettings(_reactCachingOptions.Value.CacheDuration.TotalMinutes, false, cacheKey)
        {
            AllowProgressiveCaching = true,
        };

        return await _progressiveCache.LoadAsync(async (_, _) => await loadAsync(cancellationToken), cacheSettings, cancellationToken);
    }

    public bool TryGetCachedObject<TData>(string cacheKey, [MaybeNullWhen(false)] out TData data)
    {
        if (CacheHelper.TryGetItem<TData>(cacheKey, out var cacheItem))
        {
            data = cacheItem;

            return true;
        }

        data = default;

        return false;
    }
}
