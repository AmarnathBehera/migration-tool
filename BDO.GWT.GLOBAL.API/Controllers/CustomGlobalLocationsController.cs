using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Attributes;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Dto;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Controllers;

[ApiController]
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
[Route("GlobalLocations")]
public class CustomGlobalLocationsController : EntityControllerBase
{
    private readonly ICustomGlobalLocationsService _service;

    public CustomGlobalLocationsController(ICustomGlobalLocationsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves global location(s). If a member firm ID is supplied, only the location
    /// belonging to that member firm is returned; otherwise all locations are returned.
    /// </summary>
    /// <param name="memberFirmId">ID of the member firm to filter locations by. Omit to retrieve all locations.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>200 OK with the list of matching locations.</returns>
    [HttpGet]
    public async Task<ActionResult<List<GlobalLocationCustomTableAPIDto>>> Get(
        [FromQuery] int? memberFirmId,
        CancellationToken cancellationToken)
    {
        var results = await _service.GetAllAsync(memberFirmId);
        return Ok(results);
    }

    /// <summary>
    /// Creates a new global location for the specified member firm.
    /// </summary>
    /// <param name="dto">The location details to create, including the member firm ID it belongs to.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>200 OK with the created location, or 409 Conflict if the member firm ID already has a location.</returns>
    [HttpPost]
    public async Task<ActionResult<GlobalLocationCustomTableAPIDto>> Create(
        [FromBody] GlobalLocationCustomTableUpsertDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }
        catch (DuplicateMemberFirmException ex)
        {
            return Conflict(ex.Message);
        }
        catch (GlobalLocationPersistenceException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving the location.");
        }
    }

    /// <summary>
    /// Updates the global location belonging to the specified member firm.
    /// </summary>
    /// <param name="memberFirmId">ID of the member firm whose location should be updated.</param>
    /// <param name="dto">The updated location details.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// 200 OK with the updated location, 404 Not Found if no location exists for the member firm ID,
    /// or 409 Conflict if the update would reassign the location to a member firm ID already in use.
    /// </returns>
    [HttpPut("{memberFirmId:int}")]
    public async Task<ActionResult<GlobalLocationCustomTableAPIDto>> Update(
        int memberFirmId,
        [FromBody] GlobalLocationCustomTableUpsertDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(memberFirmId, dto);

            if (result is null)
            {
                return NotFound($"No location found with MemberFirmID '{memberFirmId}'.");
            }

            return Ok(result);
        }
        catch (DuplicateMemberFirmException ex)
        {
            return Conflict(ex.Message);
        }
        catch (GlobalLocationPersistenceException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the location.");
        }
    }
}