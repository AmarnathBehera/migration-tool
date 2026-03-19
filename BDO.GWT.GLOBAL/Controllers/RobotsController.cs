using GWTXbyKWeb.Components.Services.RobotsOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BDO.GWT.Web.Controllers;
public class RobotsController : Controller
{
    private readonly IOptionsSnapshot<RobotsOptions> robotsOptions;

    public RobotsController(IOptionsSnapshot<RobotsOptions> robotsOptions)
    {
        this.robotsOptions = robotsOptions;
    }

    [HttpGet("/robots.txt")]
    public IActionResult Index() => Content(robotsOptions.Value.RobotsText, "text/plain");
}