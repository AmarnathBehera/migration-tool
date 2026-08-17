using BDO.GWT.Domain.Core.Constants;
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
[AllowAnonymous]
[ApiController]
[Route("memberFirms/{memberFirmID:int}/Users")]
public class UsersController : EntityControllerBase
{
    private readonly IUserCrudService _userCrudService;

    public UsersController(IUserCrudService userCrudService)
    {
        _userCrudService = userCrudService;
    }
    // <summary>
    // Returns all users.
    // </summary>
    // <param name="userName">User name of the user.</param>
    // <param name="cancellationToken"></param>
    // <returns>Returns a list of users.</returns>
    [HttpGet("Users")]
    [HasPermission(ApiMethodGroupName.Users, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<UserOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<UserOutput>>> Get([FromQuery] string? userName, CancellationToken cancellationToken)
    {
        var users = await _userCrudService.GetUsersAsync(userName, cancellationToken);

        return Ok(users);
    }

    // <summary>
    // Returns a single user having the specified GUID.
    // </summary>
    // <param name="userGUID">GUID of the user you want to retrieve.</param>
    // <param name="cancellationToken"></param>
    // <returns>Returns one user.</returns>
    [HttpGet("Users/{userGUID:guid}")]
    [HasPermission(ApiMethodGroupName.Users, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Get(Guid userGUID, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.GetUserAsync(userGUID, cancellationToken);

        return Ok(user);
    }

    // <summary>
    // Creates a new user.
    // </summary>
    // <param name="userInput">Input data of the new user.</param>
    // <param name="cancellationToken"></param>
    // <returns>Newly created user.</returns>
    [HttpPost("Users")]
    [HasPermission(ApiMethodGroupName.Users, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Post(int memberFirmID, [FromBody] UserInput userInput, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.CreateUserAsync(userInput, cancellationToken);

        return CreatedAtAction(nameof(Get), new { memberFirmID, userGUID = user.General.GUID }, user);
    }

    // <summary>
    // Updates a single user having the specified GUID.
    // </summary>
    // <param name="userGUID">GUID of the user.</param>
    // <param name="userInput">Updated user data.</param>
    // <param name="cancellationToken"></param>
    // <returns>Updated user.</returns>
    [HttpPut("Users/{userGUID:guid}")]
    [HasPermission(ApiMethodGroupName.Users, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(UserOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<UserOutput>> Put(Guid userGUID, [FromBody] UserInput userInput, CancellationToken cancellationToken)
    {
        var user = await _userCrudService.UpdateUserAsync(userGUID, userInput, cancellationToken);

        return Ok(user);
    }

    // <summary>
    // Deletes a single user having the specified GUID.
    // </summary>
    // <param name="userGUID">GUID of the user.</param>
    // <param name="cancellationToken"></param>
    // <returns>204 No Content.</returns>
    [HttpDelete("Users/{userGUID:guid}")]
    [HasPermission(ApiMethodGroupName.Users, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> Delete(Guid userGUID, CancellationToken cancellationToken)
    {
        await _userCrudService.DeleteUserAsync(userGUID, cancellationToken);

        return NoContent();
    }
}