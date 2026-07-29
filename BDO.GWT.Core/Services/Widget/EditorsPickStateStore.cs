using CMS.Helpers;
using CMS.ContentEngine;

namespace BDO.GWT.Core.Services;

public interface IEditorsPickStateStore
{
    void Save(
        Guid stateGuid,
        HashSet<ContentItemReference> featuredGuids,
        HashSet<ContentItemReference> selectedGuids);

    bool TryGet(
        Guid stateGuid,
        out HashSet<ContentItemReference> featuredGuids,
        out HashSet<ContentItemReference> selectedGuids);
}

/// <summary>
/// Persists the Editor's Pick configured page GUIDs keyed by the widget state GUID so that the
/// filter AJAX request (which only round-trips the state GUID) can restore them server-side.
/// </summary>
public class EditorsPickStateStore : IEditorsPickStateStore
{
    private const string CacheKeyPrefix = "BDO|EditorsPickState|";
    private const double CacheMinutes = 24 * 60;

    private readonly IProgressiveCache _progressiveCache;

    public EditorsPickStateStore(IProgressiveCache progressiveCache)
    {
        _progressiveCache = progressiveCache;
    }

    public void Save(
        Guid stateGuid,
        HashSet<ContentItemReference> featuredGuids,
        HashSet<ContentItemReference> selectedGuids)
    {
        if (stateGuid == Guid.Empty)
        {
            return;
        }

        var state = new EditorsPickState
        {
            FeaturedGuids = featuredGuids.Select(x => x.Identifier).ToArray(),
            SelectedGuids = selectedGuids.Select(x => x.Identifier).ToArray(),
        };

        var cacheSettings = new CacheSettings(CacheMinutes, GetCacheKey(stateGuid))
        {
            AllowProgressiveCaching = true,
        };

        _progressiveCache.Load(_ => state, cacheSettings);
    }

    public bool TryGet(
        Guid stateGuid,
        out HashSet<ContentItemReference> featuredGuids,
        out HashSet<ContentItemReference> selectedGuids)
    {
        featuredGuids = new HashSet<ContentItemReference>();
        selectedGuids = new HashSet<ContentItemReference>();

        if (stateGuid == Guid.Empty)
        {
            return false;
        }

        if (!CacheHelper.TryGetItem<EditorsPickState>(GetCacheKey(stateGuid), out var state)
            || state is null)
        {
            return false;
        }

        featuredGuids = ToReferences(state.FeaturedGuids);
        selectedGuids = ToReferences(state.SelectedGuids);

        return true;
    }

    private static HashSet<ContentItemReference> ToReferences(IEnumerable<Guid>? guids) =>
        (guids ?? Enumerable.Empty<Guid>())
            .Select(g => new ContentItemReference { Identifier = g })
            .ToHashSet();

    private static string GetCacheKey(Guid stateGuid) => $"{CacheKeyPrefix}{stateGuid}";

    private sealed class EditorsPickState
    {
        public Guid[] FeaturedGuids { get; set; } = Array.Empty<Guid>();

        public Guid[] SelectedGuids { get; set; } = Array.Empty<Guid>();
    }
}
