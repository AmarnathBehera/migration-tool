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
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleCrudService roleCrudService, ILogger<RolesController> logger)
    {
        _roleCrudService = roleCrudService;
        _logger = logger;
    }

    /// <summary>Returns all roles, optionally filtered by role name.</summary>
    [HttpGet("Roles")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<RoleOutput>), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleOutput>>> Get([FromQuery] string? roleName, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleCrudService.GetRolesAsync(roleName, cancellationToken);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>Returns a single role having the specified GUID.</summary>
    [HttpGet("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleOutput>> Get(Guid roleGuid, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleCrudService.GetRoleAsync(roleGuid, cancellationToken);

            return role is null
                ? Problem($"Cannot find role with GUID '{roleGuid}'.", statusCode: StatusCodes.Status404NotFound, title: "Role not found")
                : Ok(role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>Creates a new role. Role name must be unique.</summary>
    [HttpPost("Roles")]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleOutput>> Post([FromBody] RoleInput roleInput, CancellationToken cancellationToken)
    {
        try
        {
            if (roleInput is null)
                return BadRequestProblem("Request body is required.");

            var role = await _roleCrudService.CreateRoleAsync(roleInput, cancellationToken);

            return CreatedAtAction(nameof(Get), new { roleGuid = role.GUID }, role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>Updates a single role having the specified GUID.</summary>
    [HttpPut("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleOutput>> Put(Guid roleGuid, [FromBody] RoleInput roleInput, CancellationToken cancellationToken)
    {
        try
        {
            if (roleInput is null)
                return BadRequestProblem("Request body is required.");

            var role = await _roleCrudService.UpdateRoleAsync(roleGuid, roleInput, cancellationToken);
            return Ok(role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>Deletes a single role having the specified GUID.</summary>
    [HttpDelete("Roles/{roleGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid roleGuid, CancellationToken cancellationToken)
    {
        try
        {
            await _roleCrudService.DeleteRoleAsync(roleGuid, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Grants a role permissions. Applies whatever is provided: website channel permission sets
    /// (websiteChannels), email channel permission sets (emailChannels), Content hub workspace
    /// permission sets (workspaces) and/or other applications (applications).
    /// </summary>
    [HttpPost("Roles/Assign")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleOutput>> Assign([FromBody] AssignRolePermissionsInput input, CancellationToken cancellationToken)
    {
        try
        {
            if (input is null)
                return BadRequestProblem("Request body is required.");

            var role = await _roleCrudService.AssignRolePermissionsAsync(input, cancellationToken);
            return Ok(role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Creates "{WebsiteChannel}-{RoleType}" for one channel, granted the standard permission set for
    /// that role type. Returns 404 if the channel doesn't exist and 409 if the role already exists.
    /// </summary>
    [HttpPost("Roles/CreateForChannel")]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(RoleOutput), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status409Conflict)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleOutput>> CreateForChannel(
        [FromQuery] ChannelcodeNameDropdown channelcodeName,
        [FromQuery] RolesAllowedDropdown rolesallowed,
        CancellationToken cancellationToken)
    {
        try
        {
            if (channelcodeName is null || string.IsNullOrWhiteSpace(channelcodeName.Value))
                return BadRequestProblem("channelcodeName is required.");

            if (!Enum.IsDefined(rolesallowed))
                return BadRequestProblem("rolesallowed must be Editor or Administrator.");

            var input = new CreateChannelRoleInput
            {
                WebsiteChannel = channelcodeName.Value,
                RoleType = rolesallowed
            };

            var role = await _roleCrudService.CreateChannelRoleAsync(input, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Creates "{Channel}-Editor" and "{Channel}-Administrator" (if they don't already exist) for every
    /// website channel, each granted the standard permission set for that role type on its own channel.
    /// </summary>
    [HttpPost("Roles/CreateForAllChannels")]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(IEnumerable<RoleOutput>), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleOutput>>> CreateForAllChannels(CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleCrudService.CreateChannelRolesForAllChannelsAsync(cancellationToken);
            return StatusCode(StatusCodes.Status201Created, roles);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // ---------- ERROR HANDLING ----------

    private ObjectResult BadRequestProblem(string message) =>
        Problem(message, statusCode: StatusCodes.Status400BadRequest, title: "Invalid request");

    /// <summary>
    /// Maps any exception to a JSON problem response so nothing escapes the controller as an empty 500.
    /// </summary>
    private ObjectResult HandleException(Exception ex)
    {
        switch (ex)
        {
            case InvalidOperationException:
                return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Invalid request");

            case UnauthorizedAccessException:
                return Problem(ex.Message, statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");

            case ChannelNotFoundException:
                return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound, title: "Channel not found");

            case System.Collections.Generic.KeyNotFoundException:
                return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound, title: "Not found");

            case ChannelRoleExistException:
                return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Role already exists");

            case OperationCanceledException:
                return Problem("The request was cancelled.", statusCode: 499, title: "Request cancelled");

            case RolePersistenceException:
                _logger.LogError(ex, "Role persistence error.");
                return Problem(
                    $"{ex.Message} {ex.InnerException?.Message}".Trim(),
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to save role data");

            default:
                _logger.LogError(ex, "Unexpected error in RolesController.");
                return Problem(
                    "An unexpected error occurred. Please try again or contact support.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error");
        }
    }
}