using BDO;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using XperienceCommunity.Redirects.Services;

namespace XperienceCommunity.Redirects;

public class RedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRedirectService _redirectService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    private readonly string[] ExcludedStartingPaths = new[]
    {
        "/cmsctx",
        "/admin",
        "/getmedia",
        "/getcontentasset",
        "/kentico."
    };

    public RedirectMiddleware(
        RequestDelegate next,
        IRedirectService redirectService,
        IWebPageUrlRetriever webPageUrlRetriever
        )
    {
        _webPageUrlRetriever = webPageUrlRetriever;
        _next = next;
        _redirectService = redirectService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsHtmlRequest(context) || IsExcludedPath(context))
        {
            await _next(context);
            return;
        }

        string requestPath = context.Request.Path.Value?.ToLower() ?? string.Empty;

        List<RedirectInfo> allRedirects = (await _redirectService.GetRedirects()).ToList();
        RedirectInfo? matchingRedirectInfo = allRedirects
            .FirstOrDefault(r => r.RedirectSourceUrl == requestPath);

        int? targetWebPageItemId = matchingRedirectInfo is null
            ? null
            : GetWebPageItemId(matchingRedirectInfo.RedirectTargetWebPageItemGUID);

        if (!targetWebPageItemId.HasValue)
        {
            await _next(context);
            return;
        }

        string targetPageUrl = await GetTargetPageUrlAsync(context, targetWebPageItemId.Value);

        if (requestPath.Equals(targetPageUrl, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        context.Response.Redirect(targetPageUrl, permanent: true);
        await context.Response.CompleteAsync();
    }

    private static bool IsHtmlRequest(HttpContext context)
    {
        return context.Request.Headers.Accept.ToString()
            .Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsExcludedPath(HttpContext context)
    {
        return ExcludedStartingPaths.Any(excludedPath =>
            context.Request.Path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> GetTargetPageUrlAsync(HttpContext context, int targetWebPageItemId)
    {
        List<ContentLanguageInfo> languages = GetContentLangauges().ToList();
        ContentLanguageInfo currentLanguage = ResolveLanguage(context, languages);

        var result = await _webPageUrlRetriever.Retrieve(targetWebPageItemId, currentLanguage?.ContentLanguageName);
        return result.RelativePath.Replace("~", "");
    }

    private static ContentLanguageInfo ResolveLanguage(HttpContext context, List<ContentLanguageInfo> languages)
    {
        ContentLanguageInfo defaultLanguage = languages.First(l => l.ContentLanguageIsDefault);

        string firstSegment = GetFirstPathSegment(context);
        if (string.IsNullOrEmpty(firstSegment))
        {
            return defaultLanguage;
        }

        ContentLanguageInfo? matchedLanguage = languages.FirstOrDefault(l =>
            l.ContentLanguageName.Equals(firstSegment, StringComparison.CurrentCultureIgnoreCase)
            || l.ContentLanguageCultureFormat.Equals(firstSegment, StringComparison.CurrentCultureIgnoreCase));

        return matchedLanguage ?? defaultLanguage;
    }

    private static string GetFirstPathSegment(HttpContext context)
    {
        string[] segments = context.Request.Path.ToString().Split('/');
        string segment = segments.Length > 1 ? segments[1] : segments.First();
        return segment?.ToLower() ?? string.Empty;
    }

    private static int? GetWebPageItemId(Guid webPageItemGuid)
    {
        return WebPageItemInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(WebPageItemInfo.WebPageItemID))
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemGUID), webPageItemGuid)
            .GetScalarResult<int>();
    }

    private static IEnumerable<ContentLanguageInfo> GetContentLangauges()
    {
        return ContentLanguageInfo.Provider.Get()
            .Columns(nameof(ContentLanguageInfo.ContentLanguageName),
                nameof(ContentLanguageInfo.ContentLanguageCultureFormat),
                nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .ToList();
    }
}

public static class RedirectMiddlewareExtensions
{
    public static IApplicationBuilder UseXperienceCommunityRedirects(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RedirectMiddleware>();
    }
}