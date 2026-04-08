using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMS.Websites.Routing;
using BDO.GWT.Common;

namespace BDO.GWT.Xperience;

public class ScriptsHead : ViewComponent
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
    private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
    private readonly IWebsiteContext _websiteContext;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
    private const string ScriptHeadKey = "BDO_ScriptHead";

    public ScriptsHead(IInfoProvider<GlobalsettingskeyInfo> provider , IInfoProvider<WebChannelSnippetInfo> siteProvider, IWebsiteContext websiteContext, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
    {
        _provider = provider;
        _siteProvider = siteProvider;
        _websiteContext= websiteContext;
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

        var sitescriptHeadValue = _siteProvider.Get()
                         .WhereEquals(
                             nameof(WebChannelSnippetInfo.WebChannelSnippetCodeName),
                             ScriptHeadKey)
                         .WhereEquals(
                             nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                             currentChannelId.WebChannelSettingsID)
                         .TopN(1)
                         .GetEnumerableTypedResult().FirstOrDefault()?.WebChannelSnippetCode;

        var GlobalscriptHeadValue = _provider.Get()
                         .WhereEquals(
                             nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                             ScriptHeadKey)
                         .TopN(1)
                         .GetEnumerableTypedResult().FirstOrDefault()?.GlobalSettingsKeyValue;

        var finalValue = !string.IsNullOrWhiteSpace(sitescriptHeadValue)
           ? sitescriptHeadValue
           : GlobalscriptHeadValue;
       
        return View("~/Views/Shared/Components/Scripts/Default.cshtml", finalValue);
    }
}
