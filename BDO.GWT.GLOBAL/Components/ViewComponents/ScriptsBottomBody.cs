using BDO;
using BDO.GWT.Common;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class ScriptsBottomBody : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
        private readonly IWebsiteContext _websiteContext;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
        public ScriptsBottomBody(IInfoProvider<GlobalsettingskeyInfo> provider, IInfoProvider<WebChannelSnippetInfo> siteProvider, IWebsiteContext websiteContext, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
        {
            _provider = provider;
            _siteProvider = siteProvider;
            _websiteContext = websiteContext;
            _websiteChannelContext = websiteChannelContext;
            _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;

        }

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            int currentWebsiteID = _websiteChannelContext.WebsiteChannelID;
            var value = string.Empty;
            var currentChannelId = _webChannelSettingsInfoProvider
              .Get()
              .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
              .FirstOrDefault();
            if (currentChannelId == null)
            {
                return View("~/Views/Shared/Components/Scripts/Default.cshtml");
            }

            var sitescript = _siteProvider.Get()
                                    .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                                     currentChannelId.WebChannelSettingsID)
                                     .TopN(1)
                                     .GetEnumerableTypedResult()
                                     .FirstOrDefault();
            bool inheritFromGlobal = false;

            if (sitescript != null)
            {
                // Non-column fields are stored in the CustomData / ObjectData bag
                inheritFromGlobal = sitescript.ScriptinbottomofBodyInheritFromGlobal;
            }
            if (inheritFromGlobal)
            {

                value = _provider.Get()
                                   .TopN(1)
                                  .GetEnumerableTypedResult().FirstOrDefault()?.ScriptinbottomofBodyKeyValue;
            }
            else
            {
                value = sitescript?.ScriptinbottomofBodyKeyValue;

            }

            return View("~/Views/Shared/Components/Scripts/Default.cshtml", value);
        }
    }
}
