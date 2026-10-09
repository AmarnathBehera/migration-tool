using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace BDO.GWT.Web.Core.Services;

// -------------------------------------------------------------------------
// Wraps IInfoProvider<ContentLanguageInfo>.Get().GetEnumerableTypedResultAsync
// behind a simple mockable interface — ObjectQuery<T> is not mockable directly
// -------------------------------------------------------------------------
public interface ILanguageProvider
{
    Task<IReadOnlyList<string>> GetAllLanguageNamesAsync(CancellationToken cancellationToken);
}

public class LanguageProvider : ILanguageProvider
{
    private readonly IInfoProvider<ContentLanguageInfo> _infoProvider;

    public LanguageProvider(IInfoProvider<ContentLanguageInfo> infoProvider)
    {
        _infoProvider = infoProvider;
    }

    public async Task<IReadOnlyList<string>> GetAllLanguageNamesAsync(
        CancellationToken cancellationToken)
    {
        var languages = await _infoProvider
            .Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return languages
            .Select(l => l.ContentLanguageName)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();
    }
}

// -------------------------------------------------------------------------
// Wraps GetMappedWebPageResult<T> extension behind a simple mockable interface
// GetMappedWebPageResult is an extension method — not directly mockable
// -------------------------------------------------------------------------
public interface IPageNodeProvider
{
    Task<IEnumerable<XmlSitePageNode>> GetPageNodesAsync(
        string contentTypeName,
        string languageName,
        string channelName,
        CancellationToken cancellationToken);
}

public class PageNodeProvider : IPageNodeProvider
{
    private readonly IContentQueryExecutor _executor;
    private readonly ILogger<PageNodeProvider> _logger;

    public PageNodeProvider(
        IContentQueryExecutor executor,
        ILogger<PageNodeProvider> logger)
    {
        _executor = executor;
        _logger = logger;
    }

    public async Task<IEnumerable<XmlSitePageNode>> GetPageNodesAsync(
        string contentTypeName,
        string languageName,
        string channelName,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new ContentItemQueryBuilder()
                .ForContentType(contentTypeName, config => config
                    .ForWebsite(channelName)
                    .Where(w => w.WhereTrue(nameof(ISearchAndSEO.IncludeInSitemap)))
                    .WithLinkedItems(0))
                .InLanguage(languageName);

            var options = new ContentQueryExecutionOptions
            {
                ForPreview = false,
                IncludeSecuredItems = false,
            };

            var pages = await _executor.GetMappedWebPageResult<IWebPageFieldsSource>(
                query, options, cancellationToken);

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
                contentTypeName, languageName);

            return [];
        }
    }
}