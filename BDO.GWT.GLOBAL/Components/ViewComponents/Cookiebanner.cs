using BDO.GWT.Common;
using BDO.GWT.Core.DTOs.Cookieboot;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Components.ViewComponents;

public class Cookiebanner : ViewComponent
{
    private readonly ICookieBannerService _cookiebannerService;
    private readonly IInfoProvider<CookiebotInfo> _provider;
    public Cookiebanner(ICookieBannerService cookiebannerService, IInfoProvider<CookiebotInfo> provider)
    {
        _cookiebannerService = cookiebannerService;
        _provider = provider;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var setting = _provider.Get().TopN(1).FirstOrDefault();
        if (setting?.EnableCookiebot is true)
        {
            return Content(string.Empty);
        }
        var dto = await _cookiebannerService.GetAsync(cancellationToken);

        return View("~/Views/Shared/Components/Cookiebanner/Default.cshtml", dto);
    }

}