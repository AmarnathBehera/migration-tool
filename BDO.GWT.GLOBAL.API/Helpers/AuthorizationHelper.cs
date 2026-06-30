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

public interface IAuthorizationHelper
{
    Task<User?> AuthorizeUserAsync(AuthorizationFilterContext context);
}

public class AuthorizationHelper : IAuthorizationHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;


    public AuthorizationHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User?> AuthorizeUserAsync(AuthorizationFilterContext context)
    {
        var identity = context.HttpContext.User.Identity;

        if (identity == null)
        {
            context.Result = new UnauthorizedResult();

            return null;
        }

        var userName = identity.Name ?? context.HttpContext.User.FindFirst(CustomClaimTypes.AppId)?.Value;

        if (userName == null)
        {
            context.Result = new UnauthorizedResult();

            return null;
        }

        var cancellationToken = context.HttpContext.RequestAborted;
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        var user = new User();

        if (string.IsNullOrEmpty(username))
        {
            context.Result = new ForbidResult();

            return null;
        }
        else
        {
            user.PrivilegeLevel = Domain.Core.Enums.UserPrivilegeLevel.GlobalAdministrator;
        }

        return user;
    }
}
