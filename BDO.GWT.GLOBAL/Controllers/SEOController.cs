using GWTXbyKWeb.Components.Services.SEOOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BDO.GWT.Web.Controllers;
public class SEOController : Controller
{
    private readonly IOptionsSnapshot<SEOOptions> robotsOptions;

    public SEOController(IOptionsSnapshot<SEOOptions> robotsOptions)
    {
        this.robotsOptions = robotsOptions;
    }

    [HttpGet("/robots.txt")]
    public IActionResult Index() => Content(robotsOptions.Value.RobotsText, "text/plain");

    [HttpGet("/Security.txt")]
    public IActionResult Security() => Content(robotsOptions.Value.SecurityText, "text/plain");
}