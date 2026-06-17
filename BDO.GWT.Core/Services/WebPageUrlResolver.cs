using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core
{
    public class WebPageUrlResolver : IWebPageUrlResolver
    {
        private readonly IWebPageUrlRetriever _pageUrlRetriever;
        private readonly IHomePageService _homePageService;

        public WebPageUrlResolver(IWebPageUrlRetriever pageUrlRetriever, IHomePageService homePageService)
        {
            _pageUrlRetriever = pageUrlRetriever;
            _homePageService = homePageService;
        }

        public string GetRelativePath(IWebPageFieldsSource page) =>
            page.GetUrl().RelativePath;

        public string GetAbsoluteUrl(IWebPageFieldsSource page) =>
            page.GetUrl().AbsoluteUrl;

        public async Task<string> ResolvePageUrlAsync(int webPageItemId, string language, CancellationToken cancellationToken)
        {
            var urlResult = await _pageUrlRetriever.Retrieve(webPageItemId, language, cancellationToken: cancellationToken);
            return urlResult?.RelativePath?.Replace("~", "") ?? "#";
        }


        public async Task<string> GetHomePageUrlAsync(CancellationToken cancellationToken)
        {
            var homePage = await _homePageService.GetHomePageUrl(cancellationToken);
            return homePage ?? string.Empty;
        }
    }
}
