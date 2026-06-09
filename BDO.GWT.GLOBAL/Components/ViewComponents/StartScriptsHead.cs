using BDO;
using BDO.GWT.Common;
using CMS.DataEngine;
using CMS.Websites.Routing;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class StartScriptsHead : ScriptViewComponentBase
    {
        public StartScriptsHead(
            IInfoProvider<GlobalsettingskeyInfo> provider,
            IInfoProvider<WebChannelSnippetInfo> siteProvider,
            IWebsiteContext websiteContext,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
            : base(provider, siteProvider, websiteContext, websiteChannelContext, webChannelSettingsInfoProvider) { }

        protected override bool GetInheritFromGlobal(WebChannelSnippetInfo siteScript)
            => siteScript.ScriptHeadInheritfromglobalsettings;

        protected override string? GetSiteValue(WebChannelSnippetInfo siteScript)
            => siteScript.ScriptHeadKeyValue;

        protected override string? GetGlobalValue(GlobalsettingskeyInfo globalSettings)
            => globalSettings.ScriptHeadKeyValue;
    }
}
