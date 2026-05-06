using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

[Route("api/[controller]")]
public class SiteConfigurationController : Controller
{
    private readonly ISiteConfigurationService _siteConfigurationService;

    public SiteConfigurationController(ISiteConfigurationService siteConfigurationService)
    {
        _siteConfigurationService = siteConfigurationService;
    }

    [HttpGet]
    public async Task<ActionResult<SiteConfigurationDto>> GetSiteConfiguration([FromQuery] string hash, CancellationToken cancellationToken)
    {
        var cachedSiteConfiguration = _siteConfigurationService.GetCachedSiteConfigurationHash();

        if (cachedSiteConfiguration is not null && hash == cachedSiteConfiguration)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return await _siteConfigurationService.GetSiteConfigurationDtoAsync(cancellationToken);
    }
}
