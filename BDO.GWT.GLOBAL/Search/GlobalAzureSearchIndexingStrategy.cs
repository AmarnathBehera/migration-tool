using BDO;
using CMS.ContentEngine;
using CMS.DataEngine.Query;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.AzureSearch.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Search;

/// <summary>
/// Custom Azure AI Search indexing strategy that controls how BDO web pages are processed for the index.
/// <para>
/// The strategy is registered under a name (see <c>Program.cs</c>) and selected per-index in the Xperience
/// administration UI. There you also choose the channel(s), language(s) and the content types / paths that
/// should be indexed. Every matching web page item is passed to
/// <see cref="MapToAzureSearchModelOrNull(IIndexEventItemModel)"/> during a rebuild or incremental update.
/// </para>
/// <para>
/// Because <see cref="BaseAzureSearchIndexingStrategy{TSearchModel}"/> is registered as a transient service,
/// you can inject any application service you need (query executors, URL retrievers, custom services, etc.)
/// through the constructor.
/// </para>
/// </summary>
public class GlobalAzureSearchIndexingStrategy : BaseAzureSearchIndexingStrategy<GlobalSearchModel>
{
    [Obsolete("This field is retained for backward compatibility with Azure Kentico Search indexing. " +
        "Use the constructor that accepts the complete dependency set instead.")]
    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;
    private readonly IWebPageUrlRetriever urlRetriever;

    [Obsolete("This constructor is retained for backward compatibility with Azure Kentico Search indexing. " +
        "Use the constructor that accepts the complete dependency set instead.")]
    public GlobalAzureSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
        IWebPageUrlRetriever urlRetriever)
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
        this.urlRetriever = urlRetriever;
    }

    /// <summary>
    /// Maps a single content item event to a <see cref="GlobalSearchModel"/> that will be upserted into the index,
    /// or returns <c>null</c> to skip indexing the item.
    /// </summary>
    /// <remarks>
    /// The <paramref name="item"/> can represent either a web page item
    /// (<see cref="IndexEventWebPageItemModel"/>) or a reusable content item
    /// (<see cref="IndexEventReusableItemModel"/>). Here we only index web pages, and only the content types we
    /// know how to project (Insight and Service). Extend the switch to support additional content types.
    /// </remarks>
    public override async Task<IAzureSearchModel?> MapToAzureSearchModelOrNull(IIndexEventItemModel item)
    {
        if (item.IsSecured)
        {
            return null;
        }

        if (item is not IndexEventWebPageItemModel indexedPage)
        {
            return null;
        }

        var result = new GlobalSearchModel();

        if (string.Equals(indexedPage.ContentTypeName, Insight.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var page = await GetPage<Insight>(
                indexedPage.ItemGuid, indexedPage.WebsiteChannelName, indexedPage.LanguageName, Insight.CONTENT_TYPE_NAME, linkedItemsDepth: 2);

            if (page is null)
            {
                return null;
            }

            MapCommonMetadataFields(result, indexedPage, page);

            // Populate the Content field with the first non-null InsightContent from the linked items for insight pages.
            // If there are no linked InsightContent items, set Content to an empty string.
            var insightContent = page.InsightContent?.FirstOrDefault();
            
            result.Content = insightContent?.InsightContent ?? string.Empty;
            result.Image = insightContent?.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty;
            result.ImageAltText = insightContent?.ImageFileAltText ?? string.Empty;
        }
        else if (string.Equals(indexedPage.ContentTypeName, Service.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var page = await GetPage<Service>(
                indexedPage.ItemGuid, indexedPage.WebsiteChannelName, indexedPage.LanguageName, Service.CONTENT_TYPE_NAME);

            if (page is null)
            {
                return null;
            }

            MapCommonMetadataFields(result, indexedPage, page);
        }
        else if (string.Equals(indexedPage.ContentTypeName, Event.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var page = await GetPage<Event>(
                indexedPage.ItemGuid, indexedPage.WebsiteChannelName, indexedPage.LanguageName, Event.CONTENT_TYPE_NAME);

            if (page is null)
            {
                return null;
            }

            result.EventType = page.EventType.FirstOrDefault()?.Name ?? string.Empty;

            //storing event from and to date in UTC format in the index, which is important for accurate filtering and
            //sorting of search results based on event dates.
            DateTime eventFromDate = DateTime.SpecifyKind(page.EventDateFrom, DateTimeKind.Utc);
            DateTimeOffset eventFromDateOffset = new DateTimeOffset(eventFromDate);
            result.EventDateFrom = eventFromDateOffset;

            DateTime eventToDate = DateTime.SpecifyKind(page.EventDateTo, DateTimeKind.Utc);
            DateTimeOffset eventToDateOffset = new DateTimeOffset(eventToDate);

            result.EventDateTo = eventToDateOffset;
            result.EventLocationTitle = page.EventLocationTitle ?? string.Empty;
            result.EventLocationCity = page.EventLocationCity ?? string.Empty;
            result.EventLocationState = page.EventLocationState ?? string.Empty;
            result.EventLocationCountry = page.EventLocationCountry ?? string.Empty;
            result.EventContactName = page.EventContactName ?? string.Empty;
            result.EventLocationZip = page.EventLocationZip ?? string.Empty;
            result.EventContactEmail = page.EventContactEmail ?? string.Empty;
            result.EventContactPhone = page.EventContactPhone ?? string.Empty;
            result.EventIsPrivate = page.EventIsPrivate;
            result.EventLocationAddress1 = page.EventLocationAddress1 ?? string.Empty;
            result.EventLocationAddress2 = page.EventLocationAddress2 ?? string.Empty;

            MapCommonMetadataFields(result, indexedPage, page);
        }
        else
        {
            // Generic fallback: index ANY web page whose content type implements IMetadata.
            var page = await GetMetadataPageOrNull(indexedPage);
            var pageWebPageSource = await GetPageWebPageSourceOrNull(indexedPage);

            if (page is null )
            {
                // Web page, but its content type does not implement IMetadata (or not found) - skip.
                return null;
            }

            result.MetadataTitle              = page?.MetadataTitle ?? string.Empty;
            result.MetadataDescription        = page?.MetadataDescription ?? string.Empty;
            result.DocumentPageTitle          = page?.MetadataTitle ?? string.Empty;
            result.MetadataTeaserImage        = page?.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty;
            result.MetadataTeaserImageAltText = page?.MetadataTeaserImageAltText ?? string.Empty;

            SetIndexedPageFields(result, indexedPage);

            if(pageWebPageSource is not null)
            {
                result.PublishFrom =
                       pageWebPageSource?.SystemFields.ContentItemCommonDataLastPublishedWhen is DateTime from
                       ? new DateTimeOffset(from)
                       : DateTimeOffset.MinValue;

                result.PublishTo = null;
            }
           
        }

        result.Url = await GetAbsoluteUrlOrDefault(indexedPage);
        return result;
    }

    /// <summary>
    /// Maps the fields shared by all strongly typed page content models (<see cref="Insight"/>,
    /// <see cref="Service"/>, <see cref="Event"/>) that implement <see cref="IMetadata"/>,
    /// <see cref="IPageMetadata"/> and <see cref="IWebPageFieldsSource"/>.
    /// </summary>
    private void MapCommonMetadataFields<TPage>(
        GlobalSearchModel result, IndexEventWebPageItemModel indexedPage, TPage page)
        where TPage : IMetadata, IPageMetadata, IWebPageFieldsSource
    {
        result.MetadataTitle       = page.MetadataTitle ?? string.Empty;
        result.DocumentPageKeywords = page.PageKeywords ?? string.Empty;
        result.MetadataDescription = page.MetadataDescription ?? string.Empty;
        result.MetadataTeaserImage = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty;
        result.MetadataTeaserImageAltText = page.MetadataTeaserImageAltText ?? string.Empty;

        SetIndexedPageFields(result, indexedPage);

        //date time offset is used to ensure that the date is stored in UTC format in the index,
        //which is important for accurate filtering and sorting of search results based on publish dates.
        result.PublishFrom = page.SystemFields.ContentItemCommonDataLastPublishedWhen.HasValue
                            ? new DateTimeOffset(
                            DateTime.SpecifyKind(
                            page.SystemFields.ContentItemCommonDataLastPublishedWhen.Value,
                            DateTimeKind.Utc))
                           : null;
        result.PublishTo = null;
    }

    /// <summary>
    /// Maps the fields that come directly from the index event item, regardless of content type.
    /// </summary>
    private static void SetIndexedPageFields(GlobalSearchModel result, IndexEventWebPageItemModel indexedPage)
    {
        result.WebsiteChannelName  = indexedPage.WebsiteChannelName;
        result.WebPageItemTreePath = indexedPage.WebPageItemTreePath;
        result.WebPageItemGuid     = indexedPage.ItemGuid.ToString();
        result.NodeID              = indexedPage.ItemID;
    }

    /// <summary>
    /// When a web page item changes, returns the set of items that must be reindex. By default the changed
    /// item itself is reindexing. Override this to also reindex related pages (for example listing pages that
    /// display a summary of the changed page).
    /// </summary>
    public override Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem)
        => Task.FromResult<IEnumerable<IIndexEventItemModel>>(new[] { changedItem });

    private async Task<string> GetAbsoluteUrlOrDefault(IndexEventWebPageItemModel webpageItem)
    {
        try
        {
            var url = await urlRetriever.Retrieve(
                webpageItem.WebPageItemTreePath,
                webpageItem.WebsiteChannelName,
                webpageItem.LanguageName);

            return url.RelativePath;
        }
        catch (Exception)
        {
            // Retrieve can throw when a page update task is processed after the page has already been deleted.
            // In that case, fall back to an empty URL rather than failing the whole indexing batch.
            return string.Empty;
        }
    }

    /// <summary>
    /// Retrieves a strongly typed web page item by its GUID on the given channel and language.
    /// </summary>
    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName, int linkedItemsDepth = 1)
        where T : IWebPageFieldsSource, new()
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config => config
                    .WithLinkedItems(linkedItemsDepth)
                    .ForWebsite(channelName)
                    .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), id))
                    .TopN(1))
            .InLanguage(languageName);

        var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<T>);

        return result.FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the changed web page as its concrete registered content-type model and returns it as
    /// <see cref="IMetadata"/> when that content type has the Metadata reusable schema; otherwise null.
    /// </summary>
    private async Task<IMetadata?> GetMetadataPageOrNull(IndexEventWebPageItemModel indexedPage)
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(indexedPage.ContentTypeName,
                config => config
                    .WithLinkedItems(1)
                    .ForWebsite(indexedPage.WebsiteChannelName)
                    .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), indexedPage.ItemGuid))
                    .TopN(1))
            .InLanguage(indexedPage.LanguageName);

        var pages = await queryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(query);

        return pages.FirstOrDefault() as IMetadata;
    }

    private async Task<IWebPageFieldsSource?> GetPageWebPageSourceOrNull(IndexEventWebPageItemModel indexedPage)
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(indexedPage.ContentTypeName,
                config => config
                    .WithLinkedItems(1)
                    .ForWebsite(indexedPage.WebsiteChannelName)
                    .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), indexedPage.ItemGuid))
                    .TopN(1))
            .InLanguage(indexedPage.LanguageName);

        var pages = await queryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(query);

        return pages.FirstOrDefault();
    }
}
