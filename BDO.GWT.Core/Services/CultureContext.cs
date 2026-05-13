using System.Globalization;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http;

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
            var refererLanguage = GetLanguageFromReferer(context);
            if (!string.IsNullOrEmpty(refererLanguage))
                return refererLanguage;
        }

        return _preferredLanguageRetriever.Get();
    }

    private string? GetLanguageFromReferer(HttpContext context)
    {
        var referer = context.Request.Headers.Referer;

        if (string.IsNullOrEmpty(referer) || !Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            return null;

        var firstSegment = uri.AbsolutePath.Trim('/').Split('/').FirstOrDefault()?.ToLower();

        if (string.IsNullOrEmpty(firstSegment))
            return null;

        var knownLanguages = _languageProvider.Get()
            .GetEnumerableTypedResult()
            .Select(l => l.ContentLanguageName.ToLower())
            .ToHashSet();

        return knownLanguages.Contains(firstSegment) ? firstSegment : null;
    }
}