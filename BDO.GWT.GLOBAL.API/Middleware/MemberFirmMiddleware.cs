using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Websites.Routing;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Middleware;

public class MemberFirmMiddleware : IMiddleware
{
    private readonly IMemberFirmService _memberFirmService;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentService _contentRetriever;
    private readonly ICultureService _cultureService;

    public MemberFirmMiddleware(IMemberFirmService memberFirmService, IWebsiteChannelContext websiteChannelContext, IContentService contentRetriever, ICultureService cultureService)
    {
        _memberFirmService = memberFirmService;
        _websiteChannelContext = websiteChannelContext;
        _contentRetriever = contentRetriever;
        _cultureService = cultureService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.CompleteAsync();

            return;
        }

        if (!context.Request.Path.StartsWithSegments("/memberFirms", StringComparison.InvariantCultureIgnoreCase))
        {
            await next(context);

            return;
        }

        var cancellationToken = context.RequestAborted;

        var errors = new Dictionary<string, string>();

        if (!context.Request.RouteValues.ContainsKey("memberFirmID"))
        {
            errors["memberFirmID"] = "Path parameter 'memberFirmID' must be provided.";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

            return;
        }

        if (!int.TryParse((string?)context.Request.RouteValues["memberFirmID"], out var memberFirmID))
        {
            errors["memberFirmID"] = "Path parameter 'memberFirmID' value must be an integer.";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

            return;
        }

        var memberFirm = await _memberFirmService.GetMemberFirmAsync(memberFirmID, cancellationToken);
        if (memberFirm is null)
        {
            errors["memberFirmID"] = $"Member firm with ID '{memberFirmID}' not found.";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

            return;
        }

        if (memberFirm.Website == null)
        {
            throw new NullReferenceException($"Member firm with ID '{memberFirmID}' does not have 'Website' property set.");
        }

        if (string.IsNullOrEmpty(_websiteChannelContext.WebsiteChannelName))
        {
            errors["memberFirmID"] = $"Member firm with ID '{memberFirmID}' not found on this region.";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

            return;
        }

        // Extract culture from route and initialise Kentico's culture context 

        var culture = context.Request.RouteValues["culture"]?.ToString();

        if (!string.IsNullOrEmpty(culture))
        {
            try
            {
                // CultureInfo.GetCultureInfo throws CultureNotFoundException for invalid codes
                var getCulture = CultureInfo.GetCultureInfo(culture);

                CultureInfo.CurrentCulture = getCulture;
                CultureInfo.CurrentUICulture = getCulture;

            }
            catch (CultureNotFoundException)
            {
                var englishCulture = CultureInfo.GetCultureInfo(LanguageTag.GB.ToString());

                CultureInfo.CurrentCulture = englishCulture;
                CultureInfo.CurrentUICulture = englishCulture;
            }               

        }
        else
        {
            var englishCulture = CultureInfo.GetCultureInfo(LanguageTag.GB.ToString());

            CultureInfo.CurrentCulture = englishCulture;
            CultureInfo.CurrentUICulture = englishCulture;
        }

            await next(context);
    }
}
