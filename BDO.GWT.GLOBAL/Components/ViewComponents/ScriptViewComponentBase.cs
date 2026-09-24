using BDO;
using BDO.GWT.Common;
using CMS.DataEngine;
using CMS.Websites;
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
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;

        protected ScriptViewComponentBase(
            IInfoProvider<GlobalsettingskeyInfo> provider,
            IInfoProvider<WebChannelSnippetInfo> siteProvider,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
            IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider)
        {
            _provider = provider;
            _siteProvider = siteProvider;
            _websiteChannelContext = websiteChannelContext;
            _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
            _websiteChannelInfoProvider = websiteChannelInfoProvider;
        }

        protected abstract bool GetInheritFromGlobal(WebChannelSnippetInfo siteScript);

        protected abstract string GetSiteValue(WebChannelSnippetInfo siteScript);

        /// <summary>Returns the script value stored on the global settings record.</summary>
        protected abstract string GetGlobalValue(GlobalsettingskeyInfo globalSettings);

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            //int currentWebsiteID = _websiteChannelContext.WebsiteChannelID;

            

            int channelId = (await _websiteChannelInfoProvider
                    .Get()
                    .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), _websiteChannelContext.WebsiteChannelID)
                    .Columns(nameof(WebsiteChannelInfo.WebsiteChannelChannelID))
                    .TopN(1)
                    .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()?.WebsiteChannelChannelID ?? 0;

            if (channelId == 0)
            {
                return View("~/Views/Shared/Components/Scripts/Default.cshtml");
            }

            var currentChannelSettings = _webChannelSettingsInfoProvider
                .Get()
                .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), channelId)  // was currentWebsiteID (line 45)
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

            string value;
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
