using CMS.ContentEngine.Internal;

namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Wrapper interface for ContentItemInfo provider operations.
/// This abstraction exists to make IInfoProvider extension methods testable.
/// </summary>
public interface IContentItemInfoService
{
    /// <summary>
    /// Gets a ContentItemInfo by its ID.
    /// </summary>
    Task<ContentItemInfo?> GetByIdAsync(int contentItemId, CancellationToken cancellationToken = default);
}
