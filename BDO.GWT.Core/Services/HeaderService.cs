//using BDO.GWT.Domain.Entities.Pages;
//using BDO.GWT.Domain.Retrievers;
//using BDO.GWT.Domain.Services;
//using BDO.GWT.Domain.ValueObjects;
//using BDO.GWT.Web.LiveSite.DTOs;
//using BDO.GWT.Web.LiveSite.Helpers;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Web.LiveSite.Services;

//public interface IHeaderService
//{
//    Task<HeaderDto> GetAsync(ICurrentPageData currentPageData, CancellationToken cancellationToken);
//}

//public class HeaderService : HeaderServiceBase, IHeaderService
//{
//    public HeaderService(
//        ICultureSwitchService cultureSwitchService,
//        IDisplayNameService displayNameService,
//        IMultiTypePageRetriever multiTypePageRetriever,
//        IPageRetriever<HomePage> homePageRetriever,
//        IPageRetriever<Search> searchPageRetriever,
//        IShowInMenuService showInMenuService,
//        IWebsiteContext websiteContext,
//        IWebsiteCulturesService websiteCulturesService,
//        ICurrentWebsiteHelper currentWebsiteHelper) : base(cultureSwitchService, displayNameService, multiTypePageRetriever, homePageRetriever, searchPageRetriever, showInMenuService, websiteContext, websiteCulturesService, currentWebsiteHelper)
//    {
//    }
//}
