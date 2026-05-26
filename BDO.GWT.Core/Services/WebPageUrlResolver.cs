using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core
{
    public class WebPageUrlResolver : IWebPageUrlResolver
    {
        public string GetRelativePath(IWebPageFieldsSource page) =>
            page.GetUrl().RelativePath;

        public string GetAbsoluteUrl(IWebPageFieldsSource page) =>
            page.GetUrl().AbsoluteUrl;
    }
}
