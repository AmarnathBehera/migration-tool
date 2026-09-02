using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Represents the input model for creating or updating a media file in the Content Hub.
/// </summary>
public class MediaFileInput
{
    /// <summary>
    /// The file to upload.
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }

    /// <summary>
    /// Optional display name for the media file content item.
    /// If not provided, the original file name is used.
    /// </summary>
    public string? DisplayName { get; set; }
}
