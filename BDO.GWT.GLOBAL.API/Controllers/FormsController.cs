using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Controllers;

[ApiController]
[Route("Forms")]
public class FormsController : ControllerBase
{
    private readonly IFormCrudService _formCrudService;

    public FormsController(IFormCrudService formCrudService)
    {
        _formCrudService = formCrudService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormOutput>>> GetForms(CancellationToken cancellationToken)
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

    [HttpGet("{formCodeName}")]
    public async Task<ActionResult<FormOutput>> GetForm(string formCodeName, CancellationToken cancellationToken)
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

    [HttpGet("{formCodeName}/entries")]
    public async Task<ActionResult<IEnumerable<Dictionary<string, object?>>>> GetFormEntries(string formCodeName, CancellationToken cancellationToken)
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

    [HttpGet("{formCodeName}/entries/{formEntryID:int}")]
    public async Task<ActionResult<Dictionary<string, object?>>> GetFormEntry(string formCodeName, int formEntryID, CancellationToken cancellationToken)
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

    [HttpGet("{formCodeName}/entries/{formEntryID}/attachments/{formFileFieldName}")]
    public async Task<ActionResult<byte[]>> GetFormEntryAttachment(int memberFirmID, string formCodeName, int formEntryID, string formFileFieldName, CancellationToken cancellationToken)
    {
        var formEntryAttachment = await _formCrudService.GetFormEntryAttachmentAsync(formCodeName, formEntryID, formFileFieldName, cancellationToken);

        return File(formEntryAttachment.Stream, formEntryAttachment.ContentType, formEntryAttachment.FileName);
    }

    [HttpDelete("{formCodeName}/entries/{formEntryID:int}")]
    public async Task<IActionResult> DeleteFormEntry(string formCodeName, int formEntryID, CancellationToken cancellationToken)
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