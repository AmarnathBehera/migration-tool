using BDO.GWT.GLOBAL.API.Helpers;
using CMS.DataEngine;
using CMS.Membership;
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
    private readonly string _apiMethodGroupName;
    private readonly ApiPermission _permission;
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IInfoProvider<Api_permissionsInfo> _apiPermissionsProvider;
    private readonly IInfoProvider<UserInfo> _userInfoProvider;

    /// <summary>
    /// Initializes a new instance of the HasPermissionFilter class.
    /// </summary>
    /// <param name="authorizationHelper">The authorization helper instance.</param>
    /// <param name="apiPermissionsProvider">The provider for the API permissions table.</param>
    /// <param name="userInfoProvider">The provider for Kentico users.</param>
    /// <param name="apiMethodGroupName">The name of the API method group.</param>
    /// <param name="permission">The required permission level.</param>
    public HasPermissionFilter(IAuthorizationHelper authorizationHelper, IInfoProvider<Api_permissionsInfo> apiPermissionsProvider, IInfoProvider<UserInfo> userInfoProvider, string apiMethodGroupName, ApiPermission permission)
    {
        _authorizationHelper = authorizationHelper;
        _apiPermissionsProvider = apiPermissionsProvider;
        _userInfoProvider = userInfoProvider;
        _apiMethodGroupName = apiMethodGroupName;
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

        var cancellationToken = context.HttpContext.RequestAborted;

        // The permission User column stores the Kentico user ID, so resolve the caller's ID from their username.
        var currentUser = (await _userInfoProvider.Get()
                .WhereEquals(nameof(UserInfo.UserName), user.UserName)
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault()
            ?? (await _userInfoProvider.Get()
                .WhereEquals(nameof(UserInfo.Email), user.UserName)
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (currentUser is null)
        {
            context.Result = new ForbidResult();

            return;
        }

        var currentUserId = currentUser.UserID.ToString();

        // The User column is free-form long text and may hold a delimited list of user IDs.
        var groupPermissions = await _apiPermissionsProvider.Get()
            .WhereEquals(nameof(Api_permissionsInfo.MethodGroup), _apiMethodGroupName)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var userApiPermission = groupPermissions.FirstOrDefault(permission => MatchesUser(permission.User, currentUserId));

        if (userApiPermission == null)
        {
            context.Result = new UnauthorizedResult();

            return;
        }

        var allowed = _permission switch
        {
            ApiPermission.Read => userApiPermission.Read,
            ApiPermission.Manage => userApiPermission.Manage,
            _ => false,
        };

        if (!allowed)
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private static bool MatchesUser(string storedUsers, string? identifier)
    {
        if (string.IsNullOrWhiteSpace(storedUsers) || string.IsNullOrWhiteSpace(identifier))
        {
            return false;
        }

        var entries = storedUsers.Split(new[] { ';', ',', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return entries.Any(entry => string.Equals(entry, identifier, StringComparison.OrdinalIgnoreCase));
    }
}
