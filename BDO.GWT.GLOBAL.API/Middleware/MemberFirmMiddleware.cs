using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Websites.Routing;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Middleware;

/// <summary>
/// Middleware for handling member firm-related operations in the request pipeline.
/// </summary>
public class MemberFirmMiddleware : IMiddleware
{
    private readonly IMemberFirmService _memberFirmService;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    /// <summary>
    /// Initializes a new instance of the MemberFirmMiddleware class with the specified dependencies.
    /// </summary>
    /// <param name="memberFirmService">The member firm service instance.</param>
    /// <param name="websiteChannelContext">The website channel context instance.</param>
    public MemberFirmMiddleware(IMemberFirmService memberFirmService, IWebsiteChannelContext websiteChannelContext)
    {
        _memberFirmService = memberFirmService;
        _websiteChannelContext = websiteChannelContext;
    }

    /// <summary>
    /// Processes HTTP requests related to member firm operations.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
#pragma warning disable S112
            throw new NullReferenceException($"Member firm with ID '{memberFirmID}' does not have 'Website' property set.");
#pragma warning restore S112
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
