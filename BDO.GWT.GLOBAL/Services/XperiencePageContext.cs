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

    public async Task<T?> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default)
        where T : class
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var ctx))
            return null;

        if (ctx.WebPage is T typedPage)
            return typedPage;

        return null;
    }

    public async Task<MetaDataSEOItemsDto?> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default)
    {
        var canRetrieve = _webPageDataContextRetriever.TryRetrieve(out var ctx);

        if (!canRetrieve || ctx.WebPage == null)
            return null;

        var webPageItemId = ctx.WebPage.WebPageItemID;
        var request = _httpContextAccessor.HttpContext?.Request;

       
        var pages = await _contentRetriever.RetrieveAllPages<IWebPageFieldsSource>(
            new RetrieveAllPagesParameters
            {
                LinkedItemsMaxLevel = 5
            },
            query =>
            {
                query.Where(w =>
                    w.WhereEquals("WebPageItemID", webPageItemId)
                     .WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID)
                );
            },
            cacheSettings: RetrievalCacheSettings.CacheDisabled,
            configureModel: (container, p) => Task.FromResult(p),
            cancellationToken: cancellationToken
        );

        
        var page = pages?.Select(p =>
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

        }).FirstOrDefault();

        if (page == null)
            return null;

        
        if (request != null && page.OGMetaData != null)
        {
            var baseUri = new Uri($"{request.Scheme}://{request.Host}/");
            page.OGMetaData.Url = new Uri(baseUri, request.Path.Value ?? "/").ToString();

            if (!string.IsNullOrEmpty(page.OGMetaData.MetadataOGImage))
            {
                var imageUrl = page.OGMetaData.MetadataOGImage;


                if (imageUrl.StartsWith("~/"))
                {
                    imageUrl = imageUrl.Substring(2);
                }

                page.OGMetaData.MetadataOGImage = new Uri(baseUri, imageUrl).ToString();
            }
        }

        return page;
    }
}