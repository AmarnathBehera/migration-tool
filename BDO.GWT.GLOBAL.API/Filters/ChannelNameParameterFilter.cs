using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using BDO.GWT.GLOBAL.API.Services;

namespace BDO.GWT.GLOBAL.API.Filters;

public class ChannelNameParameterFilter : IParameterFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChannelNameParameterFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.Name != "channelcodeName")
            return;

        var service = _httpContextAccessor.HttpContext?.RequestServices.GetService<IRoleCrudService>();
        if (service is null)
            return;

        var names = service.GetWebsiteChannelNamesAsync(CancellationToken.None)
            .GetAwaiter().GetResult();

        parameter.Schema.Type = "string";
        parameter.Schema.Enum = names
            .Select(n => (IOpenApiAny)new OpenApiString(n))
            .ToList();
    }
}