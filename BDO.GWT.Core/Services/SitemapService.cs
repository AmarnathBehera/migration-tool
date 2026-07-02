using BDO.GWT.Core.DTOs;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using BDO.GWT.Core.Constants;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BDO.GWT.GLOBAL.Tests")]

namespace BDO.GWT.Web.Core.Services;

public interface ISitemapService
{
    Task<SitemapDto> GetSitemapDtoAsync(CancellationToken cancellationToken);
}

public class SitemapService : ISitemapService
{
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ILanguageProvider _languageProvider;
    private readonly IPageNodeProvider _pageNodeProvider;
    private readonly ILogger<SitemapService> _logger;



    public SitemapService(
        IWebPageUrlRetriever webPageUrlRetriever,
        IWebsiteChannelContext websiteChannelContext,
        ILanguageProvider languageProvider,
        IPageNodeProvider pageNodeProvider,
        ILogger<SitemapService> logger)
    {
        _webPageUrlRetriever = webPageUrlRetriever;
        _websiteChannelContext = websiteChannelContext;
        _languageProvider = languageProvider;
        _pageNodeProvider = pageNodeProvider;
        _logger = logger;
    }

    public async Task<SitemapDto> GetSitemapDtoAsync(CancellationToken cancellationToken)
    {
        var languageNames = await _languageProvider.GetAllLanguageNamesAsync(cancellationToken);

        if (languageNames.Count == 0)
        {
            _logger.LogWarning("[Sitemap] No languages found. Returning empty sitemap.");
            return new SitemapDto { Urls = [] };
        }

        var pagesByLanguage = await GetAllPagesGroupedByLanguageAsync(
            languageNames, cancellationToken);

        var urlMap = await BuildUrlMapAsync(pagesByLanguage, cancellationToken);

        var allPages = pagesByLanguage.SelectMany(x => x.Value).ToList();
        var pagesByWebPageItemId = allPages.GroupBy(p => p.WebPageItemID);

        var sitemapUrls = new List<SitemapUrlDto>();

        foreach (var group in pagesByWebPageItemId)
        {
            foreach (var page in group)
            {
                var key = BuildUrlKey(page.WebPageItemID, page.LanguageName);

                if (!urlMap.TryGetValue(key, out var absoluteUrl))
                    continue;

                var alternateUrls = group.Count() > 1
                    ? group
                        .Select(alternatePage =>
                        {
                            var alternateKey = BuildUrlKey(
                                alternatePage.WebPageItemID,
                                alternatePage.LanguageName);

                            return urlMap.TryGetValue(alternateKey, out var alternateUrl)
                                ? new SitemapAlternateUrlDto
                                {
                                    Url = alternateUrl,
                                    LanguageTag = alternatePage.LanguageName
                                }
                                : null;
                        })
                        .OfType<SitemapAlternateUrlDto>()
                        .ToList()
                    : null;

                sitemapUrls.Add(new SitemapUrlDto
                {
                    LocationUri = absoluteUrl,
                    LastModificationDate = page.LastModified,
                    AlternateUrls = alternateUrls
                });
            }
        }

        return new SitemapDto { Urls = sitemapUrls };
    }

    private async Task<Dictionary<string, List<XmlSitePageNode>>> GetAllPagesGroupedByLanguageAsync(
        IReadOnlyList<string> languageNames,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, List<XmlSitePageNode>>();
        var channelName = _websiteChannelContext.WebsiteChannelName;

        foreach (var languageName in languageNames)
        {
            var tasks = SitemapContentTypes.SitemapPages.Select(contentTypeName =>
                _pageNodeProvider.GetPageNodesAsync(
                    contentTypeName, languageName, channelName, cancellationToken));

            var pageArrays = await Task.WhenAll(tasks);
            result[languageName] = pageArrays.SelectMany(x => x).ToList();
        }

        return result;
    }

    private async Task<Dictionary<string, string>> BuildUrlMapAsync(
        Dictionary<string, List<XmlSitePageNode>> pagesByLanguage,
        CancellationToken cancellationToken)
    {
        var urlMap = new Dictionary<string, string>();

        foreach (var (languageName, pages) in pagesByLanguage)
        {
            foreach (var page in pages)
            {
                try
                {
                    var url = await _webPageUrlRetriever.Retrieve(
                        page.WebPageItemID,
                        languageName,
                        cancellationToken: cancellationToken);

                    if (!string.IsNullOrEmpty(url?.AbsoluteUrl))
                        urlMap[BuildUrlKey(page.WebPageItemID, languageName)] = url.AbsoluteUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "[Sitemap] Failed to retrieve URL for WebPageItemID: {WebPageItemID} | Language: {Language}",
                        page.WebPageItemID, languageName);
                }
            }
        }

        return urlMap;
    }

    private static string BuildUrlKey(int webPageItemId, string languageName) =>
        $"{webPageItemId}:{languageName}";
}

public sealed class XmlSitePageNode
{
    public int WebPageItemID { get; init; }
    public string LanguageName { get; init; } = null!;
    public DateTime? LastModified { get; init; }
}