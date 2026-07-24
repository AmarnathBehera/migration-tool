using BDO.GWT.GLOBAL.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Controllers;

[ApiController]
[Route("admin-api/custom-action")]
public class GdprButtonController : ControllerBase
{
    private readonly IGDPRContactLookupService gdprContactLookupService;

    public GdprButtonController(IGDPRContactLookupService gdprContactLookupService)
    {
        this.gdprContactLookupService = gdprContactLookupService;
    }

    [HttpGet("searchtotext")]
    public IActionResult SearchToText([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { result = "Email address is required." });
        }

        return Ok(new { result = gdprContactLookupService.GetContactAsText(email.Trim()) });
    }

    [HttpGet("searchtoxml")]
    public IActionResult SearchToXml([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { result = "Email address is required." });
        }

        return Ok(new { result = gdprContactLookupService.GetContactAsXml(email.Trim()) });
    }

    [HttpDelete("deleteuser")]
    public IActionResult DeleteContact([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { result = "Email address is required." });
        }

        bool deleted = gdprContactLookupService.DeleteContactByEmail(email.Trim());
        if (!deleted)
        {
            return NotFound(new { result = "No contact found to delete." });
        }

        return Ok(new { result = "Contact deleted successfully." });
    }


}
