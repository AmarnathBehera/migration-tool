using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Services;

public interface IReactCachingService
{
    Task<TData> LoadAsync<TData>(Func<CancellationToken, Task<TData>> loadAsync, string cacheKey, CancellationToken cancellationToken);

    bool TryGetCachedObject<TData>(string cacheKey, [MaybeNullWhen(false)] out TData data);
}
