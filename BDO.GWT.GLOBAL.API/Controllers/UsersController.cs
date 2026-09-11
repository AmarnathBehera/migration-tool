using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Attributes;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers;

// <summary>
// Provides methods to read and manage users.
// </summary>
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]

public class UsersController : EntityControllerBase
{
    private readonly IUserCrudService _userCrudService;
    private readonly IUserApiPermissionsCrudService _userApiPermissionsCrudService;

    public UsersController(IUserCrudService userCrudService, IUserApiPermissionsCrudService userApiPermissionsCrudService)
    {
        _userCrudService = userCrudService;
        _userApiPermissionsCrudService = userApiPermissionsCrudService;
    }
    // <summary>
    // Returns all users.
    // </summary>
    // <param name="userName">User name of the user.</param>
    // <param name="cancellationToken"></param>
    // <returns>Returns a list of users.</returns>
    [HttpGet("Users")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<UserOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<UserOutput>>> Get([FromQuery] string? userName, CancellationToken cancellationToken)
    {
        var users = await _userCrudService.GetUsersAsync(userName, cancellationToken);

        return Ok(users);
    }

    // <summary>
    // Returns a single user having the specified GUID.
    // </summary>
    // <param name="userGuid">GUID of the user you want to retrieve.</param>
    // <param name="cancellationToken"></param>
    // <returns>Returns one user.</returns>
    [HttpGet("Users/{userGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Get(Guid userGuid, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.GetUserAsync(userGuid, cancellationToken);

        return Ok(user);
    }

    // <summary>
    // Creates a new user.
    // </summary>
    // <param name="userInput">Input data of the new user.</param>
    // <param name="cancellationToken"></param>
    // <returns>Newly created user.</returns>
    [HttpPost("Users")]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Post([FromBody] UserInput userInput, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.CreateUserAsync(userInput, cancellationToken);

        return CreatedAtAction(nameof(Get), new { userGuid = user.General.GUID }, user);
    }

    // <summary>
    // Updates a single user having the specified GUID.
    // </summary>
    // <param name="userGuid">GUID of the user.</param>
    // <param name="userInput">Updated user data.</param>
    // <param name="cancellationToken"></param>
    // <returns>Updated user.</returns>
    [HttpPut("Users/{userGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Put(Guid userGuid, [FromBody] UserInput userInput, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.UpdateUserAsync(userGuid, userInput, cancellationToken);

        return Ok(user);
    }

    // <summary>
    // Deletes a single user having the specified GUID.
    // </summary>
    // <param name="userGuid">GUID of the user.</param>
    // <param name="cancellationToken"></param>
    // <returns>204 No Content.</returns>
    [HttpDelete("Users/{userGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> Delete(Guid userGuid, CancellationToken cancellationToken)
    {
        await _userCrudService.DeleteUserAsync(userGuid, cancellationToken);

        return NoContent(); 
    }
    // <summary>
    // Returns user API permissions by endpoint group name.
    // </summary>
    // <param name="memberFirmID">ID of the member firm (website channel) for which the permissions should be retrieved.</param>
    // <param name="userGuid">GUID of the user for which the permissions should be retrieved.</param>
    // <param name="cancellationToken">Cancellation token.</param>
    // <returns>User API permissions by API endpoint group name.</returns>
    [HttpGet("memberFirms/{memberFirmID:int}/Users/{userGuid:guid}/ApiPermissions")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IDictionary<string, ApiEndpointGroupPermissions>), MediaTypeNames.Application.Json)]
     public async Task<ActionResult<IDictionary<string, ApiEndpointGroupPermissions>>> GetApiPermissions(int memberFirmID, Guid userGuid, CancellationToken cancellationToken)
     {
        int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

        var userApiPermissionsByEndpointGroupName = await _userApiPermissionsCrudService.GetUserApiPermissionsByEndpointGroupNameAsync(channelId, userGuid, cancellationToken);

            return Ok(userApiPermissionsByEndpointGroupName);
        }

      // <summary>
      // Sets user API permissions by endpoint group name.
      // </summary>
      // <param name="memberFirmID">ID of the member firm (website channel) for which the permissions should be set.</param>
      // <param name="userGuid">GUID of the user for which the permissions should be set.</param>
      // <param name="userApiPermissionsByEndpointGroupNameInput">User API permissions by API endpoint group name.</param>
      // <param name="cancellationToken">Cancellation token.</param>
      // <returns>Updated user API permissions by API endpoint group name.</returns>
      [HttpPut("memberFirms/{memberFirmID:int}/Users/{userGuid:guid}/ApiPermissions")]
      [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IDictionary<string, ApiEndpointGroupPermissions>), MediaTypeNames.Application.Json)]
      public async Task<ActionResult<IDictionary<string, ApiEndpointGroupPermissions>>> SetApiPermissions(int memberFirmID, Guid userGuid, [FromBody] IDictionary<string, ApiEndpointGroupPermissions> userApiPermissionsByEndpointGroupNameInput, CancellationToken cancellationToken)
       {
        int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

        var userApiPermissionsByEndpointGroupName = await _userApiPermissionsCrudService.SetUserApiPermissionsByEndpointGroupNameAsync(channelId, userGuid, userApiPermissionsByEndpointGroupNameInput, cancellationToken);

            return Ok(userApiPermissionsByEndpointGroupName);
        }
    }
