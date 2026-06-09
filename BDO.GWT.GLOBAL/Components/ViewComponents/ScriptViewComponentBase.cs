using BDO;
using BDO.GWT.Common;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
   
    public abstract class ScriptViewComponentBase : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;

        protected ScriptViewComponentBase(
            IInfoProvider<GlobalsettingskeyInfo> provider,
            IInfoProvider<WebChannelSnippetInfo> siteProvider,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
        {
            _provider = provider;
            _siteProvider = siteProvider;
            _websiteChannelContext = websiteChannelContext;
            _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        }

        protected abstract bool GetInheritFromGlobal(WebChannelSnippetInfo siteScript);

        protected abstract string? GetSiteValue(WebChannelSnippetInfo siteScript);

        /// <summary>Returns the script value stored on the global settings record.</summary>
        protected abstract string? GetGlobalValue(GlobalsettingskeyInfo globalSettings);

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            int currentWebsiteID = _websiteChannelContext.WebsiteChannelID;

            var currentChannelSettings = _webChannelSettingsInfoProvider
                .Get()
                .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
                .FirstOrDefault();

            if (currentChannelSettings == null)
            {
                return View("~/Views/Shared/Components/Scripts/Default.cshtml");
            }

            var sitescript = _siteProvider.Get()
                .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                    currentChannelSettings.WebChannelSettingsID)
                .TopN(1)
                .GetEnumerableTypedResult()
                .FirstOrDefault();

            if (sitescript == null)
            {
                return View("~/Views/Shared/Components/Scripts/Default.cshtml");
            }

            string? value;
            if (GetInheritFromGlobal(sitescript))
            {
                var globalSettings = _provider.Get()
                    .TopN(1)
                    .GetEnumerableTypedResult()
                    .FirstOrDefault();
                value = globalSettings != null ? GetGlobalValue(globalSettings) : null;
            }
            else
            {
                value = GetSiteValue(sitescript);
            }

            return View("~/Views/Shared/Components/Scripts/Default.cshtml", value);
        }
    }
}
