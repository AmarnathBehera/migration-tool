using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Attributes;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers;

/// <summary>
/// Provides methods to read and manage license keys.
/// </summary>
/// 
[ApiController]
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
[Route("/LicenseKeys")]
public class LicenseKeysController : EntityControllerBase
{
    private readonly ILicenseKeysCrudService _licenseKeysCrudService;

    public LicenseKeysController(ILicenseKeysCrudService licenseKeysCrudService)
    {
        _licenseKeysCrudService = licenseKeysCrudService;
    }

    /// <summary>
    /// Returns all license keys.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a list of license keys.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<LicenseKeyOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<LicenseKeyOutput>>> Get(CancellationToken cancellationToken)
    {
        var licenseKeys = await _licenseKeysCrudService.GetLicenseKeyAsync( cancellationToken);

        return Ok(licenseKeys);
    }

    
}
