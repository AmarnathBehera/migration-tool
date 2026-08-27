using BDO.GWT.Core.ValueObjects;
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

        public async Task<IDictionary<int, RelativeLink>> GetPageRelativeUrlByNodeIdAsync(IEnumerable<int> nodeIds, LanguageTag languageTag, CancellationToken cancellationToken)
        {
            var linkByNodeId = new Dictionary<int, RelativeLink>();

            // Materialize once to avoid multiple enumeration.
            var distinctIds = nodeIds.Distinct();

            foreach (var webPageItemId in distinctIds)
            {
                var webPageUrl = await _pageUrlRetriever.Retrieve(
                    webPageItemId,
                    languageTag.Locale,
                    cancellationToken: cancellationToken);

                var relativePath = webPageUrl?.RelativePath;

                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                if (Link.Parse(relativePath) is RelativeLink relativeLink)
                {
                    linkByNodeId[webPageItemId] = relativeLink;
                }
            }

            return linkByNodeId;
        }
    }
}
