using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Entities;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

[Route("api/[controller]")]
public class SiteConfigurationController : Controller
{
    private readonly ILocalizationService _localizationService;

    public SiteConfigurationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<LocalizationResource> GetSiteConfiguration([FromQuery] string? hash, CancellationToken cancellationToken)
    {
        var getAllValues = await _localizationService.GetAllSiteLocalizationValues();

        //return getAllValues;
        var cachedSiteConfiguration = _siteConfigurationService.GetCachedSiteConfigurationHash();

        if (cachedSiteConfiguration is not null && hash == cachedSiteConfiguration)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return await _siteConfigurationService.GetSiteConfigurationDtoAsync(cancellationToken);
    }
}
