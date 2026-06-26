using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.Web.Core.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Core.API.Attributes;

public class AuthorizeUserPrivilegeLevelAttribute : TypeFilterAttribute
{
    public AuthorizeUserPrivilegeLevelAttribute(UserPrivilegeLevel userPrivilegeLevel) : base(typeof(UserPrivilegeLevelAuthorizationFilter))
    {
        Arguments = new object[] { userPrivilegeLevel };
    }
}
    