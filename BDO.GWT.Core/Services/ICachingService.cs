using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Services;

public interface ICachingService
{
    Task<TData> LoadAsync<TData>(Func<CancellationToken, Task<TData>> loadAction, string cacheKey, string?[] cacheDependencies, CancellationToken cancellationToken = default);

    bool TryGetCachedObject<TData>(string cacheKey, [MaybeNullWhen(false)] out TData data);
}
