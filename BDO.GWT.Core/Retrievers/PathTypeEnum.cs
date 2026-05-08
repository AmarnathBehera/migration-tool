namespace BDO.GWT.Core.Retrievers;

public enum PathTypeEnum
{
    /// <summary>
    /// Path used as it is (no additional modification).
    /// </summary>
    Explicit,
    /// <summary>
    /// Path representing single document.
    /// </summary>
    Single,
    /// <summary>
    /// Path representing all child documents.
    /// </summary>
    Children,
    /// <summary>
    /// Path representing document and its child documents.
    /// </summary>
    Section,
}
