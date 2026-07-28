using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to read and manage global settings.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("memberFirms/{memberFirmID:int}/GlobalSettings")]
public class GlobalSettingsController : EntityControllerBase
{
    private readonly IGlobalSettingCrudService _globalSettingCrudService;

    public GlobalSettingsController(IGlobalSettingCrudService globalSettingCrudService)
    {
        _globalSettingCrudService = globalSettingCrudService;
    }

    /// <summary>
    /// Returns all global settings.
    /// </summary>
    /// <param name="query">Optional query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns a list of global settings.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(GlobalSettingOutput[]), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<GlobalSettingOutput[]>> GetGlobalSettings([FromQuery] SettingsQuery query, CancellationToken cancellationToken)
    {
        var globalSettings = await _globalSettingCrudService.GetGlobalSettings(query, cancellationToken);

        return Ok(globalSettings);
    }

    /// <summary>
    /// Returns a single global setting having the specified code name.
    /// </summary>
    /// <param name="codeName">Code name of the global setting you want to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns one global setting.</returns>
    [HttpGet("{codeName}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(GlobalSettingOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<GlobalSettingOutput>> GetGlobalSetting(string codeName, CancellationToken cancellationToken)
    {
        var globalSetting = await _globalSettingCrudService.GetGlobalSetting(codeName, cancellationToken);

        return Ok(globalSetting);
    }

    /// <summary>
    /// Updates a single global setting having the specified code name.
    /// </summary>
    /// <param name="codeName">Code name of the global setting.</param>
    /// <param name="input">Updated global setting data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated global setting.</returns>
    //[HttpPut("{codeName}")]
    //[SwaggerResponse(StatusCodes.Status200OK, null, typeof(GlobalSettingOutput), MediaTypeNames.Application.Json)]
    //public async Task<ActionResult<GlobalSettingOutput>> SetGlobalSetting(string codeName, [FromBody] GlobalSettingInput input, CancellationToken cancellationToken)
    //{
    //    var globalSetting = await _globalSettingCrudService.SetGlobalSetting(codeName, input, cancellationToken);

    //    return Ok(globalSetting);
    //}
}
