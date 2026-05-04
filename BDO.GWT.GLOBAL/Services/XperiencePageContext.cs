using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Crypto;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Services;

public class XperiencePageContext : IPageContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContextRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentQueryExecutor _queryExecutor;
    private readonly IContentRetriever _contentRetriever;

    public XperiencePageContext(
        IHttpContextAccessor httpContextAccessor,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor queryExecutor,
        IContentRetriever contentRetriever)
    {
        _httpContextAccessor = httpContextAccessor;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _pageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
        _websiteChannelContext = websiteChannelContext;
        _queryExecutor = queryExecutor;
        _contentRetriever = contentRetriever;
    }

    public bool IsInPreviewMode => _websiteChannelContext.IsPreview;

    public bool IsInPageBuilderMode
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null)
                return false;

            return _pageBuilderDataContextRetriever.Retrieve().EditMode;
        }
    }

    public async Task<T> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default)
        where T : class
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var ctx))
            return null;

        var query = new ContentItemQueryBuilder()
            .ForContentType(ctx.WebPage.ContentTypeName, q => q
                .ForWebsite(ctx.WebPage.WebsiteChannelName)
                .TopN(1)
                .WithLinkedItems(1))
            .InLanguage(ctx.WebPage.LanguageName);

        var result = await _queryExecutor.GetMappedWebPageResult<T>(query, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<MetaDataSEOItemsDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default)
    {
        var canRetrieve = _webPageDataContextRetriever.TryRetrieve(out var ctx);

        if (!canRetrieve || ctx.WebPage == null)
            return null;

        var webPageItemId = ctx.WebPage.WebPageItemID;

        var pages = await _contentRetriever.RetrieveAllPages<IWebPageFieldsSource>(
            new RetrieveAllPagesParameters { LinkedItemsMaxLevel = 5 },
            query => query.Where(w =>
                w.WhereEquals("WebPageItemID", webPageItemId)
                 .WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)),
            cacheSettings: RetrievalCacheSettings.CacheDisabled,
            configureModel: (container, p) => Task.FromResult(p),
            cancellationToken: cancellationToken
        );

        var page = pages?.Select(MapToMetaDataSEOItemsDto).FirstOrDefault();

        if (page == null)
            return null;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
            ResolveOGMetaDataUrls(page, request);

        return page;
    }

    private static MetaDataSEOItemsDto MapToMetaDataSEOItemsDto(IWebPageFieldsSource p)
    {
        var result = new MetaDataSEOItemsDto();

        if (p is IMetadata metadata)
        {
            result.MetaData = new MetaDataDto
            {
                MetaTitle = metadata.MetadataTitle,
                MetaDescription = metadata.MetadataDescription,
                MetaTeaserImageAltText = metadata.MetadataTeaserImageAltText,
                MetaCanonical = metadata.MetadataCanonical,
            };
        }

        if (p is IOGMetadataFields ogMetadata)
        {
            result.OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = ogMetadata.MetadataOGTitle,
                MetadataOGDescription = ogMetadata.MetadataOGDescription,
                MetadataOGImage = ogMetadata.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            };
        }

        if (p is ISearchAndSEO seo)
        {
            result.SEOItemsDto = new SearchAndSEODto
            {
                IncludeInSitemap = seo.IncludeInSitemap,
                MetadataNofollow = seo.MetadataNofollow,
                MetadataNoindex = seo.MetadataNoindex
            };
        }

        return result;
    }

    private static void ResolveOGMetaDataUrls(MetaDataSEOItemsDto page, HttpRequest request)
    {
        if (page.OGMetaData == null)
            return;

        var baseUri = new Uri($"{request.Scheme}://{request.Host}/");
        page.OGMetaData.Url = new Uri(baseUri, request.Path.Value ?? "/").ToString();

        if (string.IsNullOrEmpty(page.OGMetaData.MetadataOGImage))
            return;

        var imageUrl = page.OGMetaData.MetadataOGImage;

        if (imageUrl.StartsWith("~/"))
            imageUrl = imageUrl.Substring(2);

        page.OGMetaData.MetadataOGImage = new Uri(baseUri, imageUrl).ToString();
    }


    /// <summary>
    /// Gets the current page's parent ID and tree path.
    /// Used to figure out where the page sits in the content tree,
    /// </summary>
    public async Task<BaseWebPage> GetCurrentPageBasePropertiesAsync(CancellationToken cancellationToken = default)
    {
        var canRetrieve = _webPageDataContextRetriever.TryRetrieve(out var ctx);
        if (!canRetrieve || ctx.WebPage == null)
        {
            return new BaseWebPage();
        }
        var query = new ContentItemQueryBuilder()

              .ForContentType(ctx.WebPage.ContentTypeName, q => q
                .ForWebsite(ctx.WebPage.WebsiteChannelName)
              .TopN(1))
              .InLanguage(ctx.WebPage.LanguageName);

        var result = await _queryExecutor.GetWebPageResult(
          query,
          item => new BaseWebPage
          {
              WebPageItemParentID = item.WebPageItemParentID,
              WebPageItemTreePath = item.WebPageItemTreePath
          },

          new ContentQueryExecutionOptions(),
          cancellationToken);
        var curPageMeta = result.FirstOrDefault() ?? new BaseWebPage();
        return curPageMeta;
    }



    /// <summary>
    /// Fetches any page by its ID, regardless of content type.
    /// Returns null if not found.
    /// </summary>
    public async Task<IWebPageFieldsSource> GetAnyPageByWebPageItemId(int webPageItemId, CancellationToken cancellationToken)
    {
        var options = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
            IncludeSecuredItems = false
        };

        var builder = new ContentItemQueryBuilder()
     .ForContentTypes(config => config
         .ForWebsite(_websiteChannelContext.WebsiteChannelName)
     )
     .Parameters(q => q
         .Where(w => w.WhereEquals(
             nameof(IWebPageContentQueryDataContainer.WebPageItemID),
             webPageItemId))
         .TopN(1)
     );

        var results = await _queryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(builder, options, cancellationToken);
        return results.FirstOrDefault();
    }


    /// <summary>
    /// Gets the display title for a page by its ID.
    /// Falls back through Title → MetadataTitle → internal page name.
    /// </summary>
    public async Task<string> GetPageTitleByWebPageItemId(int webPageItemId, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(config => config
                .ForWebsite(_websiteChannelContext.WebsiteChannelName)
            )
            .Parameters(q => q
                .Where(w => w.WhereEquals(
                    nameof(IWebPageContentQueryDataContainer.WebPageItemID), webPageItemId))
                .TopN(1)
            );

        var results = await _queryExecutor.GetWebPageResult(
            builder,
            item =>
            {
                if (item.TryGetValue("Title", out string title) && !string.IsNullOrEmpty(title))
                {
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
                }

                if (item.TryGetValue("MetadataTitle", out string metaTitle) && !string.IsNullOrEmpty(metaTitle))
                {
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(metaTitle.ToLower());
                }

                return item.WebPageItemName;
            },
            new ContentQueryExecutionOptions { ForPreview = _websiteChannelContext.IsPreview },
            cancellationToken);

        return results.FirstOrDefault();
    }

}