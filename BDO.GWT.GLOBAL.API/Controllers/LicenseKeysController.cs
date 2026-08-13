using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers;

/// <summary>
/// Provides methods to read and manage license keys.
/// </summary>
/// 
[ApiController]
//[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
[Route("memberFirms/{memberFirmID:int}/LicenseKeys")]
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

    /// <summary>
    /// Creates a new license key.
    /// </summary>
    /// <param name="licenseKeyInput">Input data of the new license key.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Newly created license key.</returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(LicenseKeyOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<LicenseKeyOutput>> Post([FromBody] LicenseKeyInput licenseKeyInput, CancellationToken cancellationToken)
    {
        var licenseKey = await _licenseKeysCrudService.SetLicenseKeyAsync(licenseKeyInput, cancellationToken);

        return CreatedAtAction(nameof(Get), new { licenseKeyID = licenseKey.ID }, licenseKey);
    }

    /// <summary>
    /// Deletes a single license key having the specified ID.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>204 No Content.</returns>
    [HttpDelete]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        await _licenseKeysCrudService.DeleteLicenseKeyAsync(cancellationToken);

        return NoContent();
    }
}
