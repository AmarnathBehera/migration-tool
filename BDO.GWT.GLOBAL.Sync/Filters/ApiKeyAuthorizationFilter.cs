using BDO.GWT.GLOBAL.Sync.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Sync.Filters;

public class ApiKeyAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IOptions<SyncAppOptions> _syncAppOptions;

    public ApiKeyAuthorizationFilter(IOptions<SyncAppOptions> syncAppOptions)
    {
        _syncAppOptions = syncAppOptions;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName.ApiKey, out var apiKey))
        {
            context.Result = new UnauthorizedResult();

            return Task.CompletedTask;
        }

        if (apiKey != _syncAppOptions.Value.SyncApiKey)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        return Task.CompletedTask;
    }
    public class HeaderName
    {
        public const string ApiKey = "X-API-Key";
    }
}