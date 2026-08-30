namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Represents the output model for a media file.
/// </summary>
public record MediaFileOutput
{
    /// <summary>
    /// Gets or sets the unique identifier of the media file.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the path/filename of the media file.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets or sets the MIME type of the media file.
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public required string Extension { get; init; }

    /// <summary>
    /// Gets or sets the GUID of the media file.
    /// </summary>
    public required Guid Guid { get; init; }

    /// <summary>
    /// Gets or sets the link to access the media file.
    /// </summary>
    public required string Link { get; init; }
}
