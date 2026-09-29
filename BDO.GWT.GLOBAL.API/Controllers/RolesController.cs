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
/// Provides methods to read and manage CMS user roles.
/// </summary>
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
public class RolesController : EntityControllerBase
{
    private readonly IRoleCrudService _roleCrudService;

    public RolesController(IRoleCrudService roleCrudService)
    {
        _roleCrudService = roleCrudService;
    }

    /// <summary>Returns all roles, optionally filtered by role name.</summary>
    [HttpGet("Roles")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<RoleOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<RoleOutput>>> Get([FromQuery] string? roleName, CancellationToken cancellationToken)
    {
        var roles = await _roleCrudService.GetRolesAsync(roleName, cancellationToken);

        return Ok(roles);
    }

    /// <summary>Returns a single role having the specified GUID.</summary>
    [HttpGet("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleOutput>> Get(Guid roleGuid, CancellationToken cancellationToken)
    {
        var role = await _roleCrudService.GetRoleAsync(roleGuid, cancellationToken);

        return role is null ? NotFound($"Cannot find role with GUID '{roleGuid}'.") : Ok(role);
    }

    /// <summary>Creates a new role. Role name must be unique.</summary>
    [HttpPost("Roles")]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<RoleOutput>> Post([FromBody] RoleInput roleInput, CancellationToken cancellationToken)
    {
        var role = await _roleCrudService.CreateRoleAsync(roleInput, cancellationToken);

        return CreatedAtAction(nameof(Get), new { roleGuid = role.GUID }, role);
    }

    /// <summary>Updates a single role having the specified GUID.</summary>
    [HttpPut("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleOutput>> Put(Guid roleGuid, [FromBody] RoleInput roleInput, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleCrudService.UpdateRoleAsync(roleGuid, roleInput, cancellationToken);
            return Ok(role);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Deletes a single role having the specified GUID.</summary>
    [HttpDelete("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid roleGuid, CancellationToken cancellationToken)
    {
        try
        {
            await _roleCrudService.DeleteRoleAsync(roleGuid, cancellationToken);
            return NoContent();
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Grants a role permissions. Applies whatever is provided: channel permission sets
    /// (websiteChannels) and/or Content hub workspace permission sets (workspaces).
    /// </summary>
    [HttpPost("Roles/Assign")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<RoleOutput>> Assign([FromBody] AssignRolePermissionsInput input, CancellationToken cancellationToken)
    {
        var role = await _roleCrudService.AssignRolePermissionsAsync(input, cancellationToken);

        return Ok(role);
    }
}