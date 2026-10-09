using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.API.Filters;

/// <summary>
/// Authorization filter for verifying user privilege levels on API requests.
/// </summary>
public class UserPrivilegeLevelAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly UserPrivilegeLevel _userPrivilegeLevel;

    /// <summary>
    /// Initializes a new instance of the UserPrivilegeLevelAuthorizationFilter class.
    /// </summary>
    /// <param name="authorizationHelper">The authorization helper instance.</param>
    /// <param name="userPrivilegeLevel">The required user privilege level.</param>
    public UserPrivilegeLevelAuthorizationFilter(IAuthorizationHelper authorizationHelper, UserPrivilegeLevel userPrivilegeLevel)
    {
        _authorizationHelper = authorizationHelper;
        _userPrivilegeLevel = userPrivilegeLevel;
    }

    /// <summary>
    /// Executes the authorization logic, checking if the user has the required privilege level.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = await _authorizationHelper.AuthorizeUserAsync(context);

        if (user is null)
        {
            return;
        }

        var allowed = user.PrivilegeLevel >= _userPrivilegeLevel;

        if (!allowed)
        {
            context.Result = new ForbidResult();
        }
    }
}
