using BDO;
using BDO.GWT.Common;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class MarketoMunchkincode : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
        private readonly IWebsiteContext _websiteContext;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
        private const string MarketoMunchkincodeKey = "BDO_MarketoMunchkincode";
        public MarketoMunchkincode(IInfoProvider<GlobalsettingskeyInfo> provider, IInfoProvider<WebChannelSnippetInfo> siteProvider, IWebsiteContext websiteContext, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
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
            var currentChannelId = _webChannelSettingsInfoProvider
              .Get()
              .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
              .FirstOrDefault();

            var siteMarketoMunchkincodeValue = _siteProvider.Get()
                             .WhereEquals(
                                 nameof(WebChannelSnippetInfo.WebChannelSnippetCodeName),
                                 MarketoMunchkincodeKey)
                             .WhereEquals(
                                 nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                                 currentChannelId.WebChannelSettingsID)
                             .TopN(1)
                             .GetEnumerableTypedResult().FirstOrDefault()?.WebChannelSnippetCode;

            var GlobalMarketoMunchkincodeValue = _provider.Get()
                             .WhereEquals(
                                 nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                                 MarketoMunchkincodeKey)
                             .TopN(1)
                             .GetEnumerableTypedResult().FirstOrDefault()?.GlobalSettingsKeyValue;

            var finalValue = !string.IsNullOrWhiteSpace(siteMarketoMunchkincodeValue)
               ? siteMarketoMunchkincodeValue
               : GlobalMarketoMunchkincodeValue;

            return View("~/Views/Shared/Components/Scripts/Default.cshtml", finalValue);
        }
    }
}
