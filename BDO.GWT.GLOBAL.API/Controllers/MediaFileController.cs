using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using System.Buffers;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to retrieve media files from the Content Hub.
/// </summary>
[ApiController]
[Route("memberFirms/{memberFirmID:int}/workspaces/{workspaceName}/mediafiles")]
public class MediaFileController : EntityControllerBase
{
    private static readonly SearchValues<char> UnsafeFileNameChars =
        SearchValues.Create(['\0', '/', '\\', '|', '<', '>', ':', '"', '?', '*']);

    private readonly IMediaFileCrudService _mediaFileCrudService;

    public MediaFileController(IMediaFileCrudService mediaFileCrudService)
    {
        _mediaFileCrudService = mediaFileCrudService;
    }

    /// <summary>
    /// Returns all media files in the specified Content Hub workspace.
    /// </summary>
    /// <param name="memberFirmID">The ID of the member firm.</param>
    /// <param name="workspaceName">The name of the Content Hub workspace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns a list of all media files in the workspace.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns all media files in the workspace.", typeof(IEnumerable<MediaFileOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<MediaFileOutput>>> GetMediaFilesAsync(
        int memberFirmID,
        string workspaceName,
        CancellationToken cancellationToken)
    {
        var mediaFiles = await _mediaFileCrudService.GetMediaFilesAsync(workspaceName, cancellationToken);

        return Ok(mediaFiles);
    }

    /// <summary>
    /// Returns a single media file for a page having the specified media file GUID.
    /// </summary>
    /// <param name="memberFirmID">The ID of the member firm.</param>
    /// <param name="workspaceName">The name of the Content Hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the media file attachment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the media file information.</returns>
    [HttpGet("{attachmentGuid:guid}", Name = "GetMediaFileByAttachmentGuid")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns the media file information.", typeof(MediaFileOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Media file not found.")]
    public async Task<ActionResult<MediaFileOutput>> GetMediaFileAsync(
        int memberFirmID,
        string workspaceName,
        Guid attachmentGuid,
        CancellationToken cancellationToken)
    {
        var mediaFile = await _mediaFileCrudService.GetMediaFileAsync(workspaceName, attachmentGuid, cancellationToken);

        if (mediaFile == null)
        {
            return NotFound();
        }

        return Ok(mediaFile);
    }

    /// <summary>
    /// Gets the binary content of a media file having the specified GUID.
    /// </summary>
    /// <param name="memberFirmID">The ID of the member firm.</param>
    /// <param name="workspaceName">The name of the Content Hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the media file attachment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the binary file content.</returns>
    [HttpGet("{attachmentGuid:guid}/binary")]
    [SwaggerOperation(Summary = "Gets media file binary content")]
    [SwaggerResponse(StatusCodes.Status200OK, "Media file binary content.", typeof(byte[]), MediaTypeNames.Application.Octet)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Media file not found.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> GetBinaryAsync(
        int memberFirmID,
        string workspaceName,
        Guid attachmentGuid,
        CancellationToken cancellationToken)
    {
        var result = await _mediaFileCrudService.DownloadMediaFileAsync(workspaceName, attachmentGuid, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        Response.GetTypedHeaders().ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = result.FileName
        };

        return File(result.Stream, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Creates a new attachment in the specified Content Hub workspace.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Creates a new attachment in the workspace")]
    [SwaggerResponse(StatusCodes.Status201Created, "Media file created.", typeof(MediaFileOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<ActionResult<MediaFileOutput>> CreateMediaFileAsync(
        [FromRoute] int memberFirmID,
        [FromRoute] string workspaceName,
        [FromForm] AttachmentRequest input,
        CancellationToken cancellationToken)
    {


        var result = await _mediaFileCrudService.CreateMediaFileAsync(workspaceName, input.File, cancellationToken);

        return CreatedAtRoute(
            "GetMediaFileByAttachmentGuid",
            new { memberFirmID, workspaceName, attachmentGuid = result.Guid },
            result);
    }

    /// <summary>
    /// Updates an existing attachment in the specified Content Hub workspace.
    /// </summary>
    [HttpPut("{attachmentGuid:guid}")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Updates an existing attachment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Media file updated.", typeof(MediaFileOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Media file not found.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<ActionResult<MediaFileOutput>> UpdateMediaFileAsync(
        [FromRoute] int memberFirmID,
        [FromRoute] string workspaceName,
        [FromRoute] Guid attachmentGuid,
        [FromForm] AttachmentRequest input,
        CancellationToken cancellationToken)
    {
        if (input?.File is null)
        {
            return BadRequest();
        }

        var file = SanitizeFile(input.File);
        var result = await _mediaFileCrudService.UpdateMediaFileAsync(workspaceName, attachmentGuid, file, cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Deletes an attachment file from the specified Content Hub workspace.
    /// </summary>
    [HttpDelete("{attachmentGuid:guid}")]
    [SwaggerOperation(Summary = "Deletes an attachment file from the workspace")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Media file deleted.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Media file not found.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error.")]
    public async Task<IActionResult> DeleteMediaFileAsync(
        [FromRoute] int memberFirmID,
        [FromRoute] string workspaceName,
        [FromRoute] Guid attachmentGuid,
        CancellationToken cancellationToken)
    {
        var deleted = await _mediaFileCrudService.DeleteMediaFileAsync(attachmentGuid, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    private static IFormFile SanitizeFile(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var safeFileName = GetSafeFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName.AsSpan().IndexOfAny(UnsafeFileNameChars) >= 0)
        {
            return file;
        }

        return new FormFile(file.OpenReadStream(), 0, file.Length, file.Name, safeFileName)
        {
            Headers = file.Headers ?? new HeaderDictionary()
        };
    }

    private static string GetSafeFileName(string? fileName)
    {
        var value = fileName ?? string.Empty;
        var separatorIndex = value.LastIndexOfAny(['/', '\\']);
        return separatorIndex >= 0 ? value[(separatorIndex + 1)..] : value;
    }
}
