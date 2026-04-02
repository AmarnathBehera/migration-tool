//using BDO.GWT.Domain.Entities.Pages;
//using BDO.GWT.Domain.Exceptions;
//using BDO.GWT.Domain.Retrievers;
//using BDO.GWT.Domain.ValueObjects;
//using BDO.GWT.Web.LiveSite.Services;
//using Kentico.Content.Web.Mvc;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Xperience.LiveSite.Services;

//public class PageContext : IPageContext
//{
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;

//    public PageContext(IMultiTypePageRetriever multiTypePageRetriever, IWebPageDataContextRetriever webPageDataContextRetriever)
//    {
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _webPageDataContextRetriever = webPageDataContextRetriever;
//    }

//    public ICurrentPageData CurrentPageData
//    {
//        get
//        {
//            var webPageDataContext = _webPageDataContextRetriever.Retrieve();

//            var webPage = webPageDataContext.WebPage;

//            var currentPageData = new CurrentPageData
//            {
//                PageID = webPage.WebPageItemID,
//                PageGUID = webPage.WebPageItemGUID,
//                LanguageID = webPage.LanguageID,
//                LanguageName = webPage.LanguageName,
//                ContentTypeID = webPage.ContentTypeID,
//                ContentTypeName = webPage.ContentTypeName,
//                WebsiteID = webPage.WebsiteChannelID,
//                WebsiteName = webPage.WebsiteChannelName,
//            };

//            return currentPageData;
//        }
//    }

//    public async Task<TAbstractPage> RetrieveCurrentPageAsync<TAbstractPage>(bool includePageTypeProperties, bool includeUrlPath, CancellationToken cancellationToken) where TAbstractPage : IPage
//    {
//        var currentPage = await _multiTypePageRetriever.RetrievePageAsync<TAbstractPage>(query =>
//        {
//            query.Where(x => x.PageID == CurrentPageData.PageID);

//            if (includePageTypeProperties)
//            {
//                query.IncludePageTypeProperties();
//            }

//            if (includeUrlPath)
//            {
//                query.IncludeUrlPath();
//            }

//        }, cancellationToken);

//        if (currentPage is null)
//        {
//            throw new FailedToRetrievePageException($"Failed to retrieve current page from the context using page ID '{CurrentPageData.PageID}'.");
//        }

//        return currentPage;
//    }
//}

//public class PageContext<TPage> : PageContext, IPageContext<TPage> where TPage : IPage
//{
//    private readonly IPageRetriever<TPage> _pageRetriever;

//    public PageContext(IMultiTypePageRetriever multiTypePageRetriever, IPageRetriever<TPage> pageRetriever, IWebPageDataContextRetriever webPageDataContextRetriever) : base(multiTypePageRetriever, webPageDataContextRetriever)
//    {
//        _pageRetriever = pageRetriever;
//    }

//    public async Task<TPage> RetrieveCurrentPageAsync(CancellationToken cancellationToken)
//    {
//        var currentPage = await _pageRetriever.RetrievePageAsync(CurrentPageData.PageID, cancellationToken);

//        if (currentPage is null)
//        {
//            throw new FailedToRetrievePageException($"Failed to retrieve current page from the context using page ID '{CurrentPageData.PageID}'.");
//        }

//        return currentPage;
//    }
//}

