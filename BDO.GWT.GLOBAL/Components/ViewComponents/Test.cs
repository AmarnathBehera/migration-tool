using BDO.GWT.Common;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience;
public class Test : ViewComponent
{
    private readonly IInfoProvider<WebChannelSnippetInfo> _provider;
    private readonly IWebsiteContext _websiteContext;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;
    private const string testKey = "test";

    public Test(IInfoProvider<WebChannelSnippetInfo> provider, IWebsiteContext websiteContext, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
    {
        _provider = provider;
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
        
        var scriptHeadValue = _provider.Get()
                         .WhereEquals(
                             nameof(WebChannelSnippetInfo.WebChannelSnippetCodeName),
                             testKey)
                         .WhereEquals(
                             nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID),
                             currentChannelId.WebChannelSettingsID)
                         .TopN(1)
                         .GetEnumerableTypedResult();
        var first = scriptHeadValue?.FirstOrDefault();
        var model = new WebChannelSnippetDTO
        {
            value = first?.WebChannelSnippetCode
        };
        return View("~/Views/Shared/Components/Scripts/Default.cshtml", model.value);
    }
}