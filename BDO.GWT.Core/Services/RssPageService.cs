using BDO.GWT.Core.Data.Rss;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;



[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BDO.GWT.Core.Tests")]

namespace BDO.GWT.Core.Services;

public interface IRssPageService
{
    Task<string> GetRssFeedXmlAsync( CancellationToken cancellationToken);
}

public class RssPageService : IRssPageService
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly ILogger<RssPageService> _logger;

    public RssPageService(
        IContentQueryExecutor contentQueryExecutor,
        IWebPageUrlRetriever webPageUrlRetriever,
        IWebsiteChannelContext websiteChannelContext,
        ICultureContext cultureContext,
        ILogger<RssPageService> logger,
        IContentService contentService)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _webPageUrlRetriever = webPageUrlRetriever;
        _websiteChannelContext = websiteChannelContext;
        _cultureContext = cultureContext;
        _contentService = contentService;
        _logger = logger;
    }

    public async Task<string> GetRssFeedXmlAsync( CancellationToken cancellationToken)
    {
        // Resolve the self-referencing URL for the RSS page itself
        // (used as <atom:link rel="self"> in the feed)
         var currentPage = await _contentService.RetrieveCurrentPageAsync<Rss>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
        if (currentPage == null)
        {
            _logger.LogWarning("[RSS] Current page is not of type Rss. Returning empty feed.");
            return string.Empty;
        }

        var rssPageUrl = await GetPageAbsoluteUrlAsync(
            currentPage.SystemFields.WebPageItemID,
            cancellationToken);

        var feed = await BuildRssFeedAsync(currentPage, cancellationToken);

        return await RssUtility.GetRssFeedXml(feed, rssPageUrl);
    }

    private async Task<RssFeed> BuildRssFeedAsync(Rss page, CancellationToken cancellationToken)
    {
        var languageName = _cultureContext.CurrentLanguageTag;

        // -------------------------------------------------------------------------
        // Step 1: Resolve the ancestor/section path from RssPath
        // RssPath is a linked web page selector — first item's TreePath defines
        // the subtree to query (e.g. "/en-gb/insights" → find all pages underneath)
        // Equivalent of KX13's NodeAliasPath.StartsWith(ancestorPage.NodeAliasPath)
        // -------------------------------------------------------------------------
        var ancestorPage = page.RssPath?.FirstOrDefault();
        var ancestorTreePath = ancestorPage?.SystemFields?.WebPageItemTreePath;

        // -------------------------------------------------------------------------
        // Step 2: Build feed channel metadata from the Rss page's fields
        // -------------------------------------------------------------------------
        var feed = new RssFeed
        {
            Channel = new RssChannel
            {
                Title = page.RssFeedTitle ?? string.Empty,
                // Channel link = the RSS page's own URL (resolved above, passed in)
                Link = await GetPageAbsoluteUrlAsync(page.SystemFields.WebPageItemID, cancellationToken),
                Description = page.RssFeedDescription ?? string.Empty,
                Language = languageName,
                Category = page.RssCategory,
            }
        };

        if (ancestorTreePath is null)
        {
            _logger.LogWarning(
                "[RSS] RssPath is not set or could not be resolved on page '{PageName}'. Returning empty feed.",
                page.SystemFields.WebPageItemName);

            return feed;
        }

        // -------------------------------------------------------------------------
        // Step 3: Query all content types under the ancestor path
        // Uses ForContentTypes + OfReusableSchema("Metadata") — the XbyK equivalent
        // of KX13's IMultiTypePageRetriever.RetrievePagesAsync<IRssItem>(
        //     query => query.Where(x => x.NodeAliasPath.StartsWith(ancestorPage.NodeAliasPath)))
        //
        // OfReusableSchema("Metadata") = IMetadata reusable schema, shared by all
        // page types — gives access to MetadataTitle, MetadataDescription, MetadataTeaserImage
        // -------------------------------------------------------------------------
        var query = new ContentItemQueryBuilder()
    .ForContentTypes(parameters => parameters
        .OfReusableSchema(IMetadata.REUSABLE_FIELD_SCHEMA_NAME)
        .ForWebsite(_websiteChannelContext.WebsiteChannelName))
    .Parameters(p => p
        .Where(w => w.WhereStartsWith(
            nameof(WebPageFields.WebPageItemTreePath), ancestorTreePath))
        .OrderBy(new OrderByColumn(
            nameof(ContentItemFields.ContentItemCommonDataLastPublishedWhen),
            OrderDirection.Descending))
        .TopN(page.RssTopN > 0 ? page.RssTopN : 10))
    .InLanguage(languageName);

        var options = new ContentQueryExecutionOptions
        {
            ForPreview = false,
            IncludeSecuredItems = false,
        };

        // -------------------------------------------------------------------------
        // Step 4: Execute query — results come back as IWebPageContentQueryDataContainer
        // We read common IMetadata columns via GetValue<T> since ForContentTypes
        // doesn't support strongly typed mapping to a single model
        // -------------------------------------------------------------------------
        IEnumerable<RssItemData> rssItems;

        try
        {
            rssItems = await _contentQueryExecutor.GetWebPageResult(
                query,
                container => new RssItemData
                {
                    WebPageItemID = container.WebPageItemID,
                    Title = container.GetValue<string>("MetadataTitle"),
                    Description = container.GetValue<string>("MetadataDescription"),
                    LastPublished = container.GetValue<DateTime?>("ContentItemCommonDataLastPublishedWhen"),
                    ImageUrl = container.GetValue<string>("MetadataTeaserImageUrl"),
                },
                options,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[RSS] Failed to query RSS items under path '{Path}' in language '{Language}'.",
                ancestorTreePath, languageName);

            return feed;
        }

        // -------------------------------------------------------------------------
        // Step 5: Resolve URLs and build RssChannelItems
        // Sequential URL resolution — same pattern as SitemapService
        // -------------------------------------------------------------------------
        foreach (var item in rssItems)
        {
            string? absoluteUrl = null;

            try
            {
                var url = await _webPageUrlRetriever.Retrieve(
                    item.WebPageItemID,
                    languageName,
                    cancellationToken: cancellationToken);

                absoluteUrl = url?.AbsoluteUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[RSS] Failed to retrieve URL for WebPageItemID: {WebPageItemID}",
                    item.WebPageItemID);
            }

            if (string.IsNullOrEmpty(absoluteUrl))
            {
                continue;
            }

            var channelItem = new RssChannelItem
            {
                Title = item.Title,
                Description = item.Description,
                PubDate = item.LastPublished.HasValue
                    ? new DateTimeOffset(item.LastPublished.Value, TimeSpan.Zero)
                    : null,
                Link = Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri) ? uri : null,
                GUID = absoluteUrl
                // Categories omitted — MetadataRssCategory not consistently present
                // Enclosure omitted — image URL format not confirmed for ContentAttachment
            };

            feed.Channel.Items.Add(channelItem);
        }

        return feed;
    }

    // -------------------------------------------------------------------------
    // Resolves an absolute URL for a given WebPageItemID in the current language
    // -------------------------------------------------------------------------
    private async Task<string> GetPageAbsoluteUrlAsync(
        int webPageItemId,
        CancellationToken cancellationToken)
    {
        var url = await _webPageUrlRetriever.Retrieve(
            webPageItemId,
            _cultureContext.CurrentLanguageTag,
            cancellationToken: cancellationToken);

        return url?.AbsoluteUrl ?? string.Empty;
    }
}

// -------------------------------------------------------------------------
// Internal projection for RSS item data
// Holds only the fields needed to build an RssChannelItem
// -------------------------------------------------------------------------
internal sealed class RssItemData
{
    public int WebPageItemID { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? LastPublished { get; init; }
    public string? ImageUrl { get; init; }
}