using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Helpers.Internal;
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


    public XperiencePageContext(
        IHttpContextAccessor httpContextAccessor,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IPageBuilderDataContextRetriever pageBuilderDataContextRetriever, IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor queryExecutor)
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

    
    // Returning null gracefully here — wire up IContentQueryExecutor for typed models.
    public async Task<T?> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default)
        where T : class
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var ctx))
            return null;

        // Cast if the context data already is T (e.g. passed through routing)
        if (ctx.WebPage is T typedPage)
            return typedPage;

        return null;
    }


    // Pull from the WebPageItemInfo fields that are available.
    public async Task<MetaDataDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default)
    {
       
        var canRetrieve = _webPageDataContextRetriever.TryRetrieve(out var ctx);

        
        if (!canRetrieve || ctx.WebPage == null)
        {
            return new MetaDataDto();
        }

        
        var contentTypeName = ctx.WebPage.ContentTypeName;

        var page = ctx.WebPage;

        var query = new ContentItemQueryBuilder()
              .ForContentType(ctx.WebPage.ContentTypeName, q => q
                .ForWebsite(ctx.WebPage.WebsiteChannelName)         
              .TopN(1))
              .InLanguage(ctx.WebPage.LanguageName);

        var result = await _queryExecutor.GetWebPageResult(
            query,
            item => new MetaDataDto
            {
                MetaTitle = item.GetValue<string>("MetadataTitle"),
                MetaDescription = item.GetValue<string>("MetadataDescription") 
            },
            new ContentQueryExecutionOptions(),
            cancellationToken);

       var curPageMeta = result.FirstOrDefault() ?? new MetaDataDto();

        return curPageMeta;
    }
}