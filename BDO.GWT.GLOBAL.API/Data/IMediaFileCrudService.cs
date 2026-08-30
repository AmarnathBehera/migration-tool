namespace BDO.GWT.GLOBAL.API.Data;

/// <summary>
/// Interface for media file CRUD service.
/// </summary>
public interface IMediaFileCrudService
{
    /// <summary>
    /// Gets all media files in the specified Content Hub workspace.
    /// </summary>
    /// <param name="workspaceName">The name of the content hub workspace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all media file outputs in the workspace.</returns>
    Task<IEnumerable<MediaFileOutput>> GetMediaFilesAsync(string workspaceName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single media file by workspace name and attachment GUID.
    /// </summary>
    /// <param name="workspaceName">The name of the content hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the attachment/media file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The media file output or null if not found.</returns>
    Task<MediaFileOutput?> GetMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken);
}
