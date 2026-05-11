using BDO.GWT.Core.ValueObjects;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace BDO.GWT.Core.LiveSite.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CultureInjectorAttribute : Attribute, IAsyncActionFilter
{
    public const string RouteParameter = "culturecode";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cultureCode = context.RouteData.Values[RouteParameter];

        if (cultureCode is string cultureCodeString && LanguageTag.TryParse(cultureCodeString, out var languageTag))
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(languageTag.ToString());
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        await next();
    }
}
