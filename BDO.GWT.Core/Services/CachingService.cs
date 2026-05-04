using BDO.GWT.Common.Options;
using CMS.Helpers;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.Web.Services;

public class CachingService : ICachingService
{
    private readonly IOptions<CachingOptions> _cachingOptions;

    public CachingService(IOptions<CachingOptions> cachingOptions)
    {
        _cachingOptions = cachingOptions;
    }

    public async Task<TData> LoadAsync<TData>(Func<CancellationToken, Task<TData>> loadAction, string cacheKey, string?[] cacheDependencies, CancellationToken cancellationToken = default)
    {
        TData? data;
        if (_cachingOptions.Value.CachingEnabled && cacheDependencies.Length > 0 && cacheDependencies.All(x => x is not null))
        {
            var cacheMinutes = _cachingOptions.Value.DefaultCacheDuration.TotalMinutes;

            var normalizedCacheDependencies = cacheDependencies.Where(x => x is not null).Select(x => x!.ToLowerInvariant()).ToArray();

            var cacheSettings = new CacheSettings(cacheMinutes, false, cacheKey)
            {
                CacheDependency = CacheHelper.GetCacheDependency(normalizedCacheDependencies),
            };

            data = await CacheHelper.CacheAsync(loadAction, cacheSettings, cancellationToken);
        }
        else
        {
            data = await loadAction(cancellationToken);
        }

        return data;
    }

    public bool TryGetCachedObject<TData>(string cacheKey, [MaybeNullWhen(false)] out TData data)
    {
        if (CacheHelper.TryGetItem<TData>(cacheKey, out var cacheItem))
        {
            data = cacheItem!;
            return true;
        }

        data = default;
        return false;
    }
}
