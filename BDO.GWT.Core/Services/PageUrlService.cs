using BDO.GWT.Core.ValueObjects;
using CMS.Websites;

namespace BDO.GWT.Core.Services;

/// <summary>
/// Resolves relative and absolute URLs for Xperience-by-Kentico web pages.
/// XbyK equivalent of the legacy Kentico 13 <c>IPageUrlService</c> that
/// used <c>IRetriever&lt;PageUrlPath&gt;</c>.
/// </summary>
public interface IPageUrlService
{
    Task<AbsoluteLink> GetPageAbsoluteUrlAsync(
        AbsoluteLink presentationUrl,
        int webPageItemId,
        string languageTag,
        CancellationToken cancellationToken);

    Task<IDictionary<string, AbsoluteLink>> GetPageAbsoluteUrlByLanguageTagAsync(
        AbsoluteLink presentationUrl,
        int webPageItemId,
        IEnumerable<string> languageTags,
        CancellationToken cancellationToken);

    Task<RelativeLink> GetPageRelativeUrlAsync(
        int webPageItemId,
        string languageTag,
        CancellationToken cancellationToken);

    Task<IDictionary<int, AbsoluteLink>> GetPageAbsoluteUrlByNodeIdAsync(
        IDictionary<AbsoluteLink, IEnumerable<int>> webPageItemIdsByPresentationUrl,
        string languageTag,
        CancellationToken cancellationToken);

    Task<IDictionary<int, RelativeLink>> GetPageRelativeUrlByNodeIdAsync(
        IEnumerable<int> webPageItemIds,
        string languageTag,
        CancellationToken cancellationToken);
}

public class PageUrlService : IPageUrlService
{
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public PageUrlService(IWebPageUrlRetriever webPageUrlRetriever)
    {
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public async Task<AbsoluteLink> GetPageAbsoluteUrlAsync(
        AbsoluteLink presentationUrl,
        int webPageItemId,
        string languageTag,
        CancellationToken cancellationToken)
    {
        var pageRelativeUrl = await GetPageRelativeUrlAsync(webPageItemId, languageTag, cancellationToken);

        return presentationUrl + pageRelativeUrl;
    }

    public async Task<IDictionary<string, AbsoluteLink>> GetPageAbsoluteUrlByLanguageTagAsync(
        AbsoluteLink presentationUrl,
        int webPageItemId,
        IEnumerable<string> languageTags,
        CancellationToken cancellationToken)
    {
        var pageRelativeUrls = await GetPageRelativeUrlByLanguageTagAsync(webPageItemId, languageTags, cancellationToken);

        return pageRelativeUrls.ToDictionary(x => x.Key, x => presentationUrl + x.Value);
    }

    public async Task<RelativeLink> GetPageRelativeUrlAsync(
        int webPageItemId,
        string languageTag,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pageUrl = await _webPageUrlRetriever.Retrieve(webPageItemId, languageTag)
            ?? throw new InvalidOperationException(
                $"Page URL of page (WebPageItemID: {webPageItemId}, Language: {languageTag}) not found.");

        return ToRelativeLink(pageUrl.RelativePath);
    }

    public async Task<IDictionary<int, AbsoluteLink>> GetPageAbsoluteUrlByNodeIdAsync(
        IDictionary<AbsoluteLink, IEnumerable<int>> webPageItemIdsByPresentationUrl,
        string languageTag,
        CancellationToken cancellationToken)
    {
        var pageAbsoluteUrlByWebPageItemId = new Dictionary<int, AbsoluteLink>();

        foreach (var (presentationUrl, webPageItemIds) in webPageItemIdsByPresentationUrl)
        {
            var pageRelativeUrlByWebPageItemId =
                await GetPageRelativeUrlByNodeIdAsync(webPageItemIds, languageTag, cancellationToken);

            foreach (var (webPageItemId, pageRelativeUrl) in pageRelativeUrlByWebPageItemId)
            {
                pageAbsoluteUrlByWebPageItemId[webPageItemId] = presentationUrl + pageRelativeUrl;
            }
        }

        return pageAbsoluteUrlByWebPageItemId;
    }

    public async Task<IDictionary<int, RelativeLink>> GetPageRelativeUrlByNodeIdAsync(
        IEnumerable<int> webPageItemIds,
        string languageTag,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var distinctIds = webPageItemIds.Distinct().ToList();

        if (distinctIds.Count == 0)
        {
            return new Dictionary<int, RelativeLink>();
        }

        var linkByWebPageItemId = new Dictionary<int, RelativeLink>(distinctIds.Count);

        foreach (var webPageItemId in distinctIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageUrl = await _webPageUrlRetriever.Retrieve(webPageItemId, languageTag);

            if (pageUrl is null)
            {
                continue;
            }

            linkByWebPageItemId[webPageItemId] = ToRelativeLink(pageUrl.RelativePath);
        }

        return linkByWebPageItemId;
    }

    private async Task<IDictionary<string, RelativeLink>> GetPageRelativeUrlByLanguageTagAsync(
        int webPageItemId,
        IEnumerable<string> languageTags,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var linkByLanguageTag = new Dictionary<string, RelativeLink>(StringComparer.OrdinalIgnoreCase);

        foreach (var languageTag in languageTags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var pageUrl = await _webPageUrlRetriever.Retrieve(webPageItemId, languageTag);

            if (pageUrl is null)
            {
                continue;
            }

            linkByLanguageTag[languageTag] = ToRelativeLink(pageUrl.RelativePath);
        }

        return linkByLanguageTag;
    }

    private static RelativeLink ToRelativeLink(string relativePath)
    {
        var lowerCasedUrlPath = (relativePath ?? string.Empty).ToLowerInvariant();

        return (RelativeLink)Link.Parse(lowerCasedUrlPath);
    }
}
