using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;

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

    /// <summary>
    /// Downloads the binary content of a media file by workspace name and attachment GUID.
    /// </summary>
    /// <param name="workspaceName">The name of the content hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the attachment/media file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="FileStreamData"/> containing the binary data, content type, and file name, or null if not found.</returns>
    Task<FileStreamData?> DownloadMediaFileAsync(string workspaceName, Guid attachmentGuid, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new media file content item in the specified Content Hub workspace.
    /// </summary>
    /// <param name="workspaceName">The name of the content hub workspace.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created media file output.</returns>
    Task<MediaFileOutput> CreateMediaFileAsync(string workspaceName, IFormFile file, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the asset of an existing media file content item.
    /// </summary>
    /// <param name="workspaceName">The name of the content hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the media file to update.</param>
    /// <param name="file">The replacement file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated media file output, or null if not found.</returns>
    Task<MediaFileOutput?> UpdateMediaFileAsync(string workspaceName, Guid attachmentGuid, IFormFile file, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a media file content item by GUID.
    /// </summary>
    /// <param name="attachmentGuid">The GUID of the media file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteMediaFileAsync(Guid attachmentGuid, CancellationToken cancellationToken);
}

