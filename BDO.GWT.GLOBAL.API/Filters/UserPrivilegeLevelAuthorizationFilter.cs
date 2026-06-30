using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.API.Filters;

public class UserPrivilegeLevelAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly UserPrivilegeLevel _userPrivilegeLevel;

    public UserPrivilegeLevelAuthorizationFilter(IAuthorizationHelper authorizationHelper, UserPrivilegeLevel userPrivilegeLevel)
    {
        _authorizationHelper = authorizationHelper;
        _userPrivilegeLevel = userPrivilegeLevel;
    }

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
