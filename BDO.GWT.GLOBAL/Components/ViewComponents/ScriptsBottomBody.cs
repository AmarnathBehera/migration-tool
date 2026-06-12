using BDO;
using BDO.GWT.Common;
using CMS.DataEngine;
using CMS.Websites.Routing;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class ScriptsBottomBody : ScriptViewComponentBase
    {
        public ScriptsBottomBody(
            IInfoProvider<GlobalsettingskeyInfo> provider,
            IInfoProvider<WebChannelSnippetInfo> siteProvider,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
            : base(provider, siteProvider, websiteChannelContext, webChannelSettingsInfoProvider) { }

        protected override bool GetInheritFromGlobal(WebChannelSnippetInfo siteScript)
            => siteScript.ScriptinbottomofBodyInheritFromGlobal;

        protected override string GetSiteValue(WebChannelSnippetInfo siteScript)
            => siteScript.ScriptinbottomofBodyKeyValue;

        protected override string GetGlobalValue(GlobalsettingskeyInfo globalSettings)
            => globalSettings.ScriptinbottomofBodyKeyValue;
    }
}
