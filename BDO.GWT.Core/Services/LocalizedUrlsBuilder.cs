using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Services;

public class LocalizedUrlsBuilder : ILocalizedUrlsBuilder
{
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageProvider;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public LocalizedUrlsBuilder(
        IInfoProvider<ContentLanguageInfo> contentLanguageProvider,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _contentLanguageProvider = contentLanguageProvider;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public async Task<IReadOnlyList<LocalizedUrl>> BuildAsync(HttpContext httpContext)
    {
        var languages = (await _contentLanguageProvider.Get().GetEnumerableTypedResultAsync())
            .OrderByDescending(l => l.ContentLanguageIsDefault)
            .ThenBy(l => l.ContentLanguageDisplayName)
            .ToList();

        var currentLanguage = _preferredLanguageRetriever.Get();
        var hasPage = _webPageDataContextRetriever.TryRetrieve(out var pageContext);
        var webPageId = hasPage ? pageContext?.WebPage?.WebPageItemID : null;

        var scheme = httpContext.Request.Scheme;
        var host = httpContext.Request.Host.HasValue ? httpContext.Request.Host.Value : "localhost";

        var result = new List<LocalizedUrl>(languages.Count);

        foreach (var language in languages)
        {
            var languageName = language.ContentLanguageName;
            var cultureFormat = language.ContentLanguageCultureFormat ?? languageName;

            var relativeUrl = await ResolveRelativeUrlAsync(webPageId, languageName, httpContext);
            var absoluteUrl = $"{scheme}://{host}{relativeUrl}";

            result.Add(new LocalizedUrl
            {
                LanguageName = languageName,
                CultureFormat = cultureFormat,
                DisplayName = language.ContentLanguageDisplayName,
                ShortCode = cultureFormat.Split('-')[0].ToUpperInvariant(),
                RelativeUrl = relativeUrl,
                AbsoluteUrl = absoluteUrl,
                IsDefault = language.ContentLanguageIsDefault,
                IsActive = string.Equals(languageName, currentLanguage, StringComparison.OrdinalIgnoreCase),
            });
        }

        return result;
    }

    private async Task<string> ResolveRelativeUrlAsync(int? webPageItemId, string languageName, HttpContext httpContext)
    {
        if (webPageItemId is int id && id > 0)
        {
            try
            {
                var pageUrl = await _webPageUrlRetriever.Retrieve(id, languageName);
                if (!string.IsNullOrEmpty(pageUrl?.RelativePath))
                {
                    return NormalizeRelativeUrl(pageUrl.RelativePath);
                }
            }
            catch
            {
                // fall through to prefix swap
            }
        }

        return SwapLanguagePrefix(httpContext, languageName);
    }

    private static string NormalizeRelativeUrl(string relativeUrl)
    {
        // Kentico's IWebPageUrlRetriever returns paths prefixed with the application virtual path token "~/"
        // (e.g. "~/fr-fr/about-us-new"). Strip the tilde so the URL becomes a regular site-relative URL.
        var normalized = relativeUrl.StartsWith("~/", StringComparison.Ordinal)
            ? relativeUrl[1..]
            : relativeUrl;

        return normalized.StartsWith('/') ? normalized : "/" + normalized;
    }

    private static string SwapLanguagePrefix(HttpContext httpContext, string languageName)
    {
        var path = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value! : "/";
        var query = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : string.Empty;

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 0)
        {
            segments[0] = languageName;
        }
        else
        {
            segments = [languageName];
        }

        return "/" + string.Join('/', segments) + query;
    }
}
