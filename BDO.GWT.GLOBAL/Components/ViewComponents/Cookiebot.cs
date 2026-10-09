using BDO.GWT.Common;
using BDO.GWT.Core.DTOs.Cookieboot;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;

public class Cookiebot : ViewComponent
{
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
    private readonly IInfoProvider<WebChannelSnippetInfo> _siteProvider;

    public Cookiebot(
        IInfoProvider<GlobalsettingskeyInfo> provider,
        IInfoProvider<WebChannelSnippetInfo> siteProvider)
    {
        _provider = provider;
        _siteProvider = siteProvider;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var data = _siteProvider.Get().TopN(1).FirstOrDefault();
        var cookiebotDto = new CookiebotDTO();

        if (data == null)
        {
            return View("~/Components/Views/Shared/Components/Cookiebot/Default.cshtml", cookiebotDto);
        }

        // Only fetch global data once, and only if at least one field needs it
        GlobalsettingskeyInfo globalData = null;
        if (data.EnableCookiebotInheritfromglobal
            || data.CookieIDInheritfromglobal
            || data.CustomCookiebotScriptInheritfromglobal)
        {
            globalData = _provider.Get().TopN(1).FirstOrDefault();
        }

        cookiebotDto.EnableCookiebot = data.EnableCookiebotInheritfromglobal
            ? globalData?.EnableCookiebot ?? false
            : data.EnableCookiebot;

        cookiebotDto.CookieID = data.CookieIDInheritfromglobal
            ? globalData?.CookieID
            : data.CookieID;

        cookiebotDto.CustomCookiebotScript = data.CustomCookiebotScriptInheritfromglobal
            ? globalData?.CustomCookiebotScript
            : data.CustomCookiebotScript;

        return View("~/Components/Views/Shared/Components/Cookiebot/Default.cshtml", cookiebotDto);
    }
}