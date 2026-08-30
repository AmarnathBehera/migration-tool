using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to retrieve media files from the Content Hub.
/// </summary>
[ApiController]
[Route("memberFirms/{memberFirmID:int}/workspaces/{workspaceName}/mediafiles")]
public class MediaFileController : EntityControllerBase
{
    private readonly IMediaFileCrudService _mediaFileCrudService;

    public MediaFileController(IMediaFileCrudService mediaFileCrudService)
    {
        _mediaFileCrudService = mediaFileCrudService;
    }

    /// <summary>
    /// Returns a single media file for a page having the specified media file GUID.
    /// </summary>
    /// <param name="memberFirmID">The ID of the member firm.</param>
    /// <param name="workspaceName">The name of the Content Hub workspace.</param>
    /// <param name="attachmentGuid">The GUID of the media file attachment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the media file information.</returns>
    [HttpGet("{attachmentGuid:guid}")]
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
}
