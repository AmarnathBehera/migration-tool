using BDO.GWT.Core;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BDO.GWT.GLOBAL.API.Filters;

/// <summary>
/// Attribute for specifying required API permissions on controller actions.
/// </summary>
public class HasPermissionAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the HasPermissionAttribute class with the specified API method group and permission.
    /// </summary>
    /// <param name="apiMethodGroupName">The name of the API method group.</param>
    /// <param name="permission">The required permission level.</param>
    public HasPermissionAttribute(string apiMethodGroupName, ApiPermission permission) : base(typeof(HasPermissionFilter))
    {
        Arguments = new object[] { apiMethodGroupName, permission };
    }
}

/// <summary>
/// Authorization filter for verifying API permissions on requests.
/// </summary>
public class HasPermissionFilter : IAsyncAuthorizationFilter
{
    private readonly ApiPermission _permission;
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    /// Initializes a new instance of the HasPermissionFilter class.
    /// </summary>
    /// <param name="authorizationHelper">The authorization helper instance.</param>
    /// <param name="apiMethodGroupName">The name of the API method group.</param>
    /// <param name="permission">The required permission level.</param>
    public HasPermissionFilter(IAuthorizationHelper authorizationHelper, string apiMethodGroupName, ApiPermission permission)
    {
        _authorizationHelper = authorizationHelper;
        _permission = permission;
    }

    /// <summary>
    /// Executes the authorization logic to verify if the user has the required permission.
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

        var methodGroups = new List<ApiMethodGroup>
        {
            new ApiMethodGroup { ID = 1, Name = "weather" }
        };

        var methodGroup = methodGroups.SingleOrDefault();

        if (methodGroup == null)
        {
            context.Result = new ForbidResult();

            return;
        }

        var userApiPermissions = new List<UserApiPermission>
        {
            new UserApiPermission { UserID = user.ID, ApiMethodGroupID = methodGroup.ID, AllowRead = true, AllowManage = false }
        };

        var userApiPermission = userApiPermissions.SingleOrDefault();

        if (userApiPermission == null)
        {
            context.Result = new ForbidResult();

            return;
        }

        var allowed = _permission switch
        {
            ApiPermission.Read => userApiPermission.AllowRead,
            ApiPermission.Manage => userApiPermission.AllowManage,
            _ => false,
        };

        if (!allowed)
        {
            context.Result = new ForbidResult();
        }
    }
}
