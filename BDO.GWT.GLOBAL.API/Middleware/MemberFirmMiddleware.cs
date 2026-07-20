using BDO.GWT.Common.Core.Extensions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Middleware;

/// <summary>
/// Middleware for handling member firm-related operations in the request pipeline.
/// </summary>
public class MemberFirmMiddleware : IMiddleware
{
    private readonly IGlobalLocationCustomTableDataService _globalLocationCustomTableDataService;

    /// <summary>
    /// Initializes a new instance of the MemberFirmMiddleware class with the specified dependencies.
    /// </summary>
    /// <param name="globalLocationCustomTableDataService">The global location custom table data service instance.</param>
    public MemberFirmMiddleware(IGlobalLocationCustomTableDataService globalLocationCustomTableDataService)
    {
        _globalLocationCustomTableDataService = globalLocationCustomTableDataService;
    }

    /// <summary>
    /// Processes HTTP requests related to member firm operations.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        //if (context.User.Identity is not { IsAuthenticated: true })
        //{
        //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //    await context.Response.CompleteAsync();

        //    return;
        //}

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
        var globalLocationCustomTableData = await _globalLocationCustomTableDataService.GetLocationsAsync(memberFirmID);

        if (globalLocationCustomTableData != null)
        {

            context.Items["ChannelId"] = globalLocationCustomTableData.ChannelId == 0 ? 1 : globalLocationCustomTableData.ChannelId;
        }
        else
        {
            errors["memberFirmID"] = $"Member firm with ID '{memberFirmID}' not found.";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

            return;
        }
        await next(context);
    }
}
