using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
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


    public XperiencePageContext(
        IHttpContextAccessor httpContextAccessor,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor queryExecutor
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _pageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
        _websiteChannelContext = websiteChannelContext;
        _queryExecutor = queryExecutor;
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
            .InLanguage(ctx.WebPage.LanguageName)
        .Parameters(q => q
        .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), ctx.WebPage.WebPageItemID))
    );

        var result = await _queryExecutor.GetMappedWebPageResult<T>(query, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<MetaDataSEOItemsDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default)
    {
        var canRetrieve = _webPageDataContextRetriever.TryRetrieve(out var ctx);

        if (!canRetrieve || ctx.WebPage == null)
            return null;


        var webPageItemId = ctx.WebPage.WebPageItemID;
        // Get the content type name from context - avoids querying ALL types
        var contentTypeName = ctx.WebPage.ContentTypeName;

        //use the content type name to query only pages of tha particular content type.
        var builder = new ContentItemQueryBuilder()
       .ForContentType(
           contentTypeName,
           subQuery => subQuery
               .ForWebsite(ctx.WebPage.WebsiteChannelName)
               .Where(w => w
                   .WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemID), webPageItemId)
               )
               .WithLinkedItems(5)
       )
       .InLanguage(ctx.WebPage.LanguageName);

        var options = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview
        };

        var pages = await _queryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(
            builder, options, cancellationToken);

        var page = pages?.Select(MapToMetaDataSEOItemsDto).FirstOrDefault();

        if (page == null)
            return null;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
            ResolveOGMetaDataUrls(page, request);

        return page;

    }

    private static MetaDataSEOItemsDto CreateEmptyMetaDataSEOItemsDto() => new()
    {
        MetaData = new MetaDataDto(),
        OGMetaData = new OGMetaDataDto(),
        SEOItemsDto = new SearchAndSEODto(),
        PageMetaData = new PageMetaDataDto()
    };

    private static MetaDataSEOItemsDto MapToMetaDataSEOItemsDto(IWebPageFieldsSource p)
    {
        var result = CreateEmptyMetaDataSEOItemsDto();

        if (p is IMetadata metadata)
        {
            result.MetaData.MetaTitle = metadata.MetadataTitle;
            result.MetaData.MetaDescription = metadata.MetadataDescription;
            result.MetaData.MetaTeaserImageAltText = metadata.MetadataTeaserImageAltText;
            result.MetaData.MetaCanonical = metadata.MetadataCanonical;
        }

        if (p is IOGMetadataFields ogMetadata)
        {
            result.OGMetaData.MetadataOGTitle = ogMetadata.MetadataOGTitle;
            result.OGMetaData.MetadataOGDescription = ogMetadata.MetadataOGDescription;
            result.OGMetaData.MetadataOGImage = ogMetadata.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        }

        if (p is ISearchAndSEO seo)
        {
            result.SEOItemsDto.IncludeInSitemap = seo.IncludeInSitemap;
            result.SEOItemsDto.MetadataNofollow = seo.MetadataNofollow;
            result.SEOItemsDto.MetadataNoindex = seo.MetadataNoindex;
        }

        if (p is IPageMetadata pageMetaData)
        {
            result.PageMetaData.PageTitle = pageMetaData.PageTitle;
            result.PageMetaData.PageDescription = pageMetaData.PageDescription;
            result.PageMetaData.PageKeywords = pageMetaData.PageKeywords;
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
                .Where(w => w.WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemID), ctx.WebPage.WebPageItemID))
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
                    return title;
                }

                if (item.TryGetValue("MetadataTitle", out string metaTitle) && !string.IsNullOrEmpty(metaTitle))
                {
                    return metaTitle;
                }

                return item.WebPageItemName;
            },
            new ContentQueryExecutionOptions { ForPreview = _websiteChannelContext.IsPreview },
            cancellationToken);

        return results.FirstOrDefault();
    }

}