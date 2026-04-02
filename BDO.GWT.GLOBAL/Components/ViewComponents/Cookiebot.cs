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
    private readonly IInfoProvider<CookiebotInfo> _provider;
    public Cookiebot(IInfoProvider<CookiebotInfo> provider)
    {
        _provider = provider;
       
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {

        var data = _provider.Get().TopN(1).FirstOrDefault();

        var cookiebotDto = new CookiebotDTO();

        if (data != null)
        {
            cookiebotDto.EnableCookiebot = data.EnableCookiebot;
            cookiebotDto.CookieID = data.CookieID;
            cookiebotDto.CustomCookiebotScript = data.CustomCookiebotScript;
        }
        return View("~/Components/Views/Shared/Components/Cookiebot/Default.cshtml",cookiebotDto);
    }


}