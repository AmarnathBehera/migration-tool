using System.Globalization;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BDO.GWT.Core.Services;

public interface ICultureContext
{
    public string CurrentLanguageTag { get; }
    public bool IsCurrentCultureRtl { get; }
}

public class CultureContext : ICultureContext
{
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IInfoProvider<ContentLanguageInfo> _languageProvider;

    public CultureContext(
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IHttpContextAccessor httpContextAccessor,
        IInfoProvider<ContentLanguageInfo> languageProvider)
    {
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _httpContextAccessor = httpContextAccessor;
        _languageProvider = languageProvider;
    }

    public string CurrentLanguageTag => ResolveLanguage();

    public bool IsCurrentCultureRtl =>
        CultureInfo.GetCultureInfo(ResolveLanguage()).TextInfo.IsRightToLeft;

    private string ResolveLanguage()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            var routeLanguage = GetLanguageFromRoute(context);
            if (!string.IsNullOrEmpty(routeLanguage))
            {
                return routeLanguage;
            }
        }

        return _preferredLanguageRetriever.Get();
    }

    private string? GetLanguageFromRoute(HttpContext context)
    {
        var routeLanguage = context.GetRouteValue(WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY)?.ToString()?.ToLowerInvariant();

        var knownLanguages = _languageProvider.Get()
            .GetEnumerableTypedResult()
            .Select(l => l.ContentLanguageCultureFormat.ToLower())
            .ToHashSet();

        return !string.IsNullOrEmpty(routeLanguage) && knownLanguages.Contains(routeLanguage) ? routeLanguage : null;
    }
}