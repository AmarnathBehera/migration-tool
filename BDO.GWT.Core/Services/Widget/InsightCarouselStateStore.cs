using CMS.ContentEngine;
using CMS.Helpers;

namespace BDO.GWT.Core.Services;

public interface IInsightCarouselStateStore
{
    void Save(Guid stateGuid, HashSet<ContentItemReference> featuredGuids);

    bool TryGet(Guid stateGuid, out HashSet<ContentItemReference> featuredGuids);
}

/// <summary>
/// Persists Insight carousel featured item GUIDs keyed by widget state GUID so filter API requests
/// can restore featured flags after client-side filtering.
/// </summary>
public class InsightCarouselStateStore : IInsightCarouselStateStore
{
    private const string CacheKeyPrefix = "BDO|InsightCarouselState|";
    private const double CacheMinutes = 24 * 60;

    private readonly IProgressiveCache _progressiveCache;

    public InsightCarouselStateStore(IProgressiveCache progressiveCache)
    {
        _progressiveCache = progressiveCache;
    }

    public void Save(Guid stateGuid, HashSet<ContentItemReference> featuredGuids)
    {
        if (stateGuid == Guid.Empty)
        {
            return;
        }

        var state = new InsightCarouselState
        {
            FeaturedGuids = (featuredGuids ?? new HashSet<ContentItemReference>())
                .Select(x => x.Identifier)
                .ToArray(),
        };

        var cacheSettings = new CacheSettings(CacheMinutes, GetCacheKey(stateGuid))
        {
            AllowProgressiveCaching = true,
        };

        _progressiveCache.Load(_ => state, cacheSettings);
    }

    public bool TryGet(Guid stateGuid, out HashSet<ContentItemReference> featuredGuids)
    {
        featuredGuids = new HashSet<ContentItemReference>();

        if (stateGuid == Guid.Empty)
        {
            return false;
        }

        if (!CacheHelper.TryGetItem<InsightCarouselState>(GetCacheKey(stateGuid), out var state)
            || state is null)
        {
            return false;
        }

        featuredGuids = state.FeaturedGuids
            .Select(g => new ContentItemReference { Identifier = g })
            .ToHashSet();

        return true;
    }

    private static string GetCacheKey(Guid stateGuid) => $"{CacheKeyPrefix}{stateGuid}";

    private sealed class InsightCarouselState
    {
        public Guid[] FeaturedGuids { get; set; } = Array.Empty<Guid>();
    }
}
