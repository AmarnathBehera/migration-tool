using BDO.GWT.Core;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BDO.GWT.GLOBAL.API.Filters;

public class HasPermissionAttribute : TypeFilterAttribute
{
    public HasPermissionAttribute(string apiMethodGroupName, ApiPermission permission) : base(typeof(HasPermissionFilter))
    {
        Arguments = new object[] { apiMethodGroupName, permission };
    }
}

public class HasPermissionFilter : IAsyncAuthorizationFilter
{
    //private readonly string _apiMethodGroupName;
    private readonly ApiPermission _permission;
    private readonly IAuthorizationHelper _authorizationHelper;
    //private readonly IRetriever<ApiMethodGroup> _apiMethodGroupRetriever;
    //private readonly IRetriever<UserApiPermission> _userApiPermissionRetriever;


    public HasPermissionFilter(IAuthorizationHelper authorizationHelper, string apiMethodGroupName, ApiPermission permission) //IRetriever<ApiMethodGroup> apiMethodGroupRetriever, IRetriever<UserApiPermission> userApiPermissionRetriever
    {
        //_apiMethodGroupRetriever = apiMethodGroupRetriever;
        //_apiMethodGroupName = apiMethodGroupName;
        _authorizationHelper = authorizationHelper;
        _permission = permission;
        //_userApiPermissionRetriever = userApiPermissionRetriever;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = await _authorizationHelper.AuthorizeUserAsync(context);

        if (user is null)
        {
            return;
        }

        var cancellationToken = context.HttpContext.RequestAborted;

        //var methodGroups = await _apiMethodGroupRetriever.RetrieveEntitiesAsync(query => query.Where(x => x.Name == _apiMethodGroupName), cancellationToken);

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

        //var userApiPermissions = await _userApiPermissionRetriever.RetrieveEntitiesAsync(query => query.Where(x => x.UserID == user.ID && x.ApiMethodGroupID == methodGroup.ID), cancellationToken);
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
