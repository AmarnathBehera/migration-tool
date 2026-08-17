using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers;

/// <summary>
/// Provides methods to read and manage forms.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("memberFirms/{memberFirmID:int}/Forms")]
public class FormsController : EntityControllerBase
{
    private readonly IFormCrudService _formCrudService;

    public FormsController(IFormCrudService formCrudService)
    {
        _formCrudService = formCrudService;
    }

    /// <summary>
    /// Returns all forms.
    /// </summary>
    /// /// <param name="memberFirmID">ID of the member firm from which the forms should be retrieved.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a list of forms.</returns>
    [HttpGet]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<FormOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<FormOutput>>> GetForms(int memberFirmID, CancellationToken cancellationToken)
    {
        try
        {
            var forms = await _formCrudService.GetFormsAsync(cancellationToken);
            return Ok(forms);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving forms: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a single form having the specified code name.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which the form should be retrieved.</param>
    /// <param name="formCodeName">Code name of the form you want to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one form.</returns>
    [HttpGet("{formCodeName}")]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(FormOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<FormOutput>> GetForm(int memberFirmID, string formCodeName, CancellationToken cancellationToken)
    {
        try
        {
            var form = await _formCrudService.GetFormAsync(formCodeName, cancellationToken);
            return Ok(form);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving form: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns all form entries.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which you want to retrieve form entries.</param>
    /// <param name="formCodeName">Code name of the form.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns all form entries.</returns>
    [HttpGet("{formCodeName}/entries")]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<Dictionary<string, object?>>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<Dictionary<string, object?>>>> GetFormEntries(int memberFirmID, string formCodeName, CancellationToken cancellationToken)
    {
        try
        {
            var formEntries = await _formCrudService.GetFormEntriesAsync(formCodeName, cancellationToken);
            return Ok(formEntries);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving form entries: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a single form entry.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which you want to retrieve form entry.</param>
    /// <param name="formCodeName">Code name of the form.</param>
    /// <param name="formEntryID">ID of the form entry.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one form entry.</returns>
    [HttpGet("{formCodeName}/entries/{formEntryID}")]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Dictionary<string, object?>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<Dictionary<string, object?>>> GetFormEntry(int memberFirmID, string formCodeName, int formEntryID, CancellationToken cancellationToken)
    {
        try
        {
            var formEntry = await _formCrudService.GetFormEntryAsync(formCodeName, formEntryID, cancellationToken);
            return Ok(formEntry);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving form entry: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a form entry attachment.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which you want to retrieve form entry attachment.</param>
    /// <param name="formCodeName">Code name of the form.</param>
    /// <param name="formEntryID">ID of the form entry.</param>
    /// <param name="formFileFieldName">Name (code name) of the form field which uses 'File uploader' form component.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns file content.</returns>
    [HttpGet("{formCodeName}/entries/{formEntryID}/attachments/{formFileFieldName}")]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(byte[]), MediaTypeNames.Application.Octet)]
    public async Task<ActionResult<byte[]>> GetFormEntryAttachment(int memberFirmID, string formCodeName, int formEntryID, string formFileFieldName, CancellationToken cancellationToken)
    {
        var formEntryAttachment = await _formCrudService.GetFormEntryAttachmentAsync(formCodeName, formEntryID, formFileFieldName, cancellationToken);

        return File(formEntryAttachment.Stream, formEntryAttachment.ContentType, formEntryAttachment.FileName);
    }

    /// <summary>
    /// Deletes a single form entry.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm in which you want to delete form entry.</param>
    /// <param name="formCodeName">Code name of the form.</param>
    /// <param name="formEntryID">ID of the form entry.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Deletes one form entry.</returns>
    [HttpDelete("{formCodeName}/entries/{formEntryID}")]
    [HasPermission(ApiMethodGroupName.Forms, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void), MediaTypeNames.Application.Json)]
    public async Task<IActionResult> DeleteFormEntry(int memberFirmID, string formCodeName, int formEntryID, CancellationToken cancellationToken)
    {
        try
        {
            await _formCrudService.DeleteFormEntryAsync(formCodeName, formEntryID, cancellationToken);
            return Ok(new { message = $"Form entry '{formEntryID}' for form '{formCodeName}' was successfully deleted." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting form entry: {ex.Message}");
        }
    }
}