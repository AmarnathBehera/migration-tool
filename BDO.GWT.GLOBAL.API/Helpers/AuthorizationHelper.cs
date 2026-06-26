using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Retrievers;
using BDO.GWT.Web.Core.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.API.Helpers;

public interface IAuthorizationHelper
{
    Task<User?> AuthorizeUserAsync(AuthorizationFilterContext context);
}

public class AuthorizationHelper : IAuthorizationHelper
{
    private readonly IRetriever<User> _userRetriever;

    public AuthorizationHelper(IRetriever<User> userRetriever)
    {
        _userRetriever = userRetriever;
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

        var users = await _userRetriever.RetrieveEntitiesAsync(query => query.Global().Where(x => x.UserName == userName), cancellationToken);

        var user = users.SingleOrDefault();

        if (user == null)
        {
            context.Result = new ForbidResult();

            return null;
        }

        return user;
    }
}
