using BDO.GWT.GLOBAL.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GdprContactLookupController : ControllerBase
{
    private readonly IGDPRContactLookupService gdprContactLookupService;

    public GdprContactLookupController(IGDPRContactLookupService gdprContactLookupService)
    {
        this.gdprContactLookupService = gdprContactLookupService;
    }

    [HttpGet("text")]
    public IActionResult SearchToText([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { result = "Email address is required." });
        }

        return Ok(new { result = gdprContactLookupService.GetContactAsText(email.Trim()) });
    }

    [HttpGet("xml")]
    public IActionResult SearchToXml([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { result = "Email address is required." });
        }

        return Ok(new { result = gdprContactLookupService.GetContactAsXml(email.Trim()) });
    }
}