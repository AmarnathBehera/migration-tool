using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using BDO.GWT.Core.Services;

namespace BDO.GWT.Web.Core.Services;

public interface ISitemapService
{
    Task<SitemapDto> GetSitemapDtoAsync(CancellationToken cancellationToken);
}

public class SitemapService : ISitemapService
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<ContentLanguageInfo> _languageInfoProvider;
    private readonly ILogger<SitemapService> _logger;

    // -------------------------------------------------------------------------
    // Typed registry — each entry maps a content type to a query delegate
    // that returns SitemapPageNodes with LastModified from SystemFields.
    // Add new page types here; no other changes needed.
    // -------------------------------------------------------------------------
    private IReadOnlyList<Func<string, CancellationToken, Task<IEnumerable<XmlSitePageNode>>>> BuildPageTypeQueries() =>
    [
        (lang, ct) => QueryPageTypeAsync<Career>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<Careers>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SectionCareers>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<Sitemap>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SectionIndustries>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<Industry>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<IndustryService>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<IndustryCategory>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SpecialtiesArea>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SpecialtiesCategory>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SpecialtiesPage>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SectionSpecialties>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<ServiceArea>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<BusinessLine>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<Sub_Service>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<SectionServices>(lang, ct),
        (lang, ct) => QueryPageTypeAsync<Service>(lang, ct),
    ];

    public SitemapService(
        IContentQueryExecutor contentQueryExecutor,
        IWebPageUrlRetriever webPageUrlRetriever,
        IWebsiteChannelContext websiteChannelContext,
        IInfoProvider<ContentLanguageInfo> languageInfoProvider,
        ILogger<SitemapService> logger)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _webPageUrlRetriever = webPageUrlRetriever;
        _websiteChannelContext = websiteChannelContext;
        _languageInfoProvider = languageInfoProvider;
        _logger = logger;
    }

    public async Task<SitemapDto> GetSitemapDtoAsync(CancellationToken cancellationToken)
    {
        // Step 1: Get all configured languages from Xperience (global — not per channel)
        var languages = await GetAllLanguagesAsync(cancellationToken);

        if (languages.Count == 0)
        {
            _logger.LogWarning("[Sitemap] No languages found. Returning empty sitemap.");
            return new SitemapDto { Urls = [] };
        }

        var pagesByLanguage = await GetAllPagesGroupedByLanguageAsync(languages, cancellationToken);

        var urlMap = await BuildUrlMapAsync(pagesByLanguage, cancellationToken);


        var allPages = pagesByLanguage.SelectMany(x => x.Value).ToList();
        var pagesByWebPageItemId = allPages.GroupBy(p => p.WebPageItemID);

        // Step 5: Build SitemapUrlDto list
        var sitemapUrls = new List<SitemapUrlDto>();

        foreach (var group in pagesByWebPageItemId)
        {
            foreach (var page in group)
            {
                var key = BuildUrlKey(page.WebPageItemID, page.LanguageName);

                if (!urlMap.TryGetValue(key, out var absoluteUrl))
                {
                    continue;
                }

                var alternateUrls = group.Count() > 1
                    ? group
                        .Select(alternatePage =>
                        {
                            var alternateKey = BuildUrlKey(alternatePage.WebPageItemID, alternatePage.LanguageName);
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

    private async Task<IReadOnlyList<ContentLanguageInfo>> GetAllLanguagesAsync(
        CancellationToken cancellationToken)
    {
        var languages = await _languageInfoProvider
            .Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return languages.ToList();
    }

    private async Task<Dictionary<string, List<XmlSitePageNode>>> GetAllPagesGroupedByLanguageAsync(
        IReadOnlyList<ContentLanguageInfo> languages,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, List<XmlSitePageNode>>();
        var queries = BuildPageTypeQueries();

        foreach (var language in languages)
        {
            var languageName = language.ContentLanguageName;

            // Parallel across page types — safe because queries are read-only
            var tasks = queries.Select(query => query(languageName, cancellationToken));
            var pageArrays = await Task.WhenAll(tasks);
            result[languageName] = pageArrays.SelectMany(x => x).ToList();
        }

        return result;
    }


    private async Task<IEnumerable<XmlSitePageNode>> QueryPageTypeAsync<T>(
        string languageName,
        CancellationToken cancellationToken)
        where T : class, IWebPageFieldsSource, ISearchAndSEO, new()
    {
        try
        {
            var query = new ContentItemQueryBuilder()
                .ForContentType(
                    // CONTENT_TYPE_NAME is a static const on every generated class —
                    // safe to access via new() + typeof pattern
                    typeof(T).GetField("CONTENT_TYPE_NAME")?.GetValue(null)?.ToString()
                        ?? throw new InvalidOperationException($"CONTENT_TYPE_NAME not found on {typeof(T).Name}"),
                    config => config
                        .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                        .Where(where => where
                            .WhereTrue(nameof(ISearchAndSEO.IncludeInSitemap)))
                        .WithLinkedItems(0))
                .InLanguage(languageName);

            var options = new ContentQueryExecutionOptions
            {
                ForPreview = false,
                IncludeSecuredItems = false,
            };

            var pages = await _contentQueryExecutor.GetMappedWebPageResult<T>(
                query,
                options,
                cancellationToken);

            return pages.Select(page => new XmlSitePageNode
            {
                WebPageItemID = page.SystemFields.WebPageItemID,
                LanguageName = languageName,

                LastModified = page.SystemFields.ContentItemCommonDataLastPublishedWhen,
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[Sitemap] Failed to query '{ContentType}' in language '{Language}'.",
                typeof(T).Name, languageName);

            return [];
        }
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
                    {
                        urlMap[BuildUrlKey(page.WebPageItemID, languageName)] = url.AbsoluteUrl;
                    }
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

internal sealed class XmlSitePageNode
{
    public int WebPageItemID { get; init; }
    public string LanguageName { get; init; } = null!;
    public DateTime? LastModified { get; init; }
}
