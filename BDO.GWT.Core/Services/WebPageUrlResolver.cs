using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core
{
    public class WebPageUrlResolver : IWebPageUrlResolver
    {
        private readonly IWebPageUrlRetriever _pageUrlRetriever;

        public WebPageUrlResolver(IWebPageUrlRetriever pageUrlRetriever)
        {
            _pageUrlRetriever = pageUrlRetriever;
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
    }
}
