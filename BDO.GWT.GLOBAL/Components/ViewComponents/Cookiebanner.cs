using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;

public class Cookiebanner : ViewComponent
{
    private readonly ICookieBannerService _cookiebannerService;
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
    private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> _webChannelSettingsInfoProvider;

    public Cookiebanner(
        ICookieBannerService cookiebannerService,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IInfoProvider<GlobalsettingskeyInfo> provider,
        IInfoProvider<WebChannelSnippetInfo> siteProvider)
    {
        _cookiebannerService = cookiebannerService;
        _provider = provider;
        _siteProvider = siteProvider;
        _websiteChannelContext = websiteChannelContext;
        _webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var channelSetting = _siteProvider.Get().TopN(1).FirstOrDefault();

        bool enableCookiebot;
        if (channelSetting != null && channelSetting.EnableCookiebotInheritfromglobal)
        {
            var globalSetting = _provider.Get().TopN(1).FirstOrDefault();
            enableCookiebot = globalSetting?.EnableCookiebot ?? false;
        }
        else
        {
            enableCookiebot = channelSetting?.EnableCookiebot ?? false;
        }

        if (enableCookiebot)
        {
            return Content(string.Empty);
        }

        var dto = await _cookiebannerService.GetAsync(cancellationToken);
        return View("~/Views/Shared/Components/Cookiebanner/Default.cshtml", dto);
    }
}