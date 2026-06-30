using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Attributes;

public class AuthorizeUserPrivilegeLevelAttribute : TypeFilterAttribute
{
    public AuthorizeUserPrivilegeLevelAttribute(UserPrivilegeLevel userPrivilegeLevel) : base(typeof(UserPrivilegeLevelAuthorizationFilter))
    {
        Arguments = new object[] { userPrivilegeLevel };
    }
}
    