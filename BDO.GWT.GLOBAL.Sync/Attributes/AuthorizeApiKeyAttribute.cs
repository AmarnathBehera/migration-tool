using BDO.GWT.GLOBAL.Sync.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.Sync.Attributes;

public class AuthorizeApiKeyAttribute : TypeFilterAttribute
{
    public AuthorizeApiKeyAttribute() : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}
