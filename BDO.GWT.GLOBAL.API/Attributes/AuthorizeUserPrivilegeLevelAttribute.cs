using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Attributes;

/// <summary>
/// Attribute for authorizing requests based on user privilege levels.
/// </summary>
public class AuthorizeUserPrivilegeLevelAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the AuthorizeUserPrivilegeLevelAttribute class with the specified privilege level.
    /// </summary>
    /// <param name="userPrivilegeLevel">The required privilege level for authorization.</param>
    public AuthorizeUserPrivilegeLevelAttribute(UserPrivilegeLevel userPrivilegeLevel) : base(typeof(UserPrivilegeLevelAuthorizationFilter))
    {
        Arguments = new object[] { userPrivilegeLevel };
    }
}
    