using BDO.GWT.Core;
using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Retrievers;
using CMS.Membership;
using CMS.Membership.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.API.Helpers;

/// <summary>
/// Interface for the authorization helper that provides methods for authorizing users in the application.
/// </summary>
public interface IAuthorizationHelper
{
    /// <summary>
    /// Authorizes a user based on the provided authorization filter context.
    /// </summary>
    /// <param name="context">The authorization filter context containing the HTTP context and user identity.</param>
    /// <returns>A task that returns the authorized user if successful; otherwise, null.</returns>
    Task<User?> AuthorizeUserAsync(AuthorizationFilterContext context);
}

/// <summary>
/// Implementation of the authorization helper interface.
/// </summary>
public class AuthorizationHelper : IAuthorizationHelper
{
    /// <inheritdoc />
    public Task<User?> AuthorizeUserAsync(AuthorizationFilterContext context)
    {
        var identity = context.HttpContext.User.Identity;

        var userName = identity?.Name ?? context.HttpContext.User.FindFirst(CustomClaimTypes.AppId)?.Value;

        if (string.IsNullOrEmpty(userName))
        {
            context.Result = new UnauthorizedResult();

            return Task.FromResult<User?>(null);
        }

        var user = new User
        {
            UserName = userName,
            PrivilegeLevel = Domain.Core.Enums.UserPrivilegeLevel.GlobalAdministrator,
        };

        return Task.FromResult<User?>(user);
    }
}
