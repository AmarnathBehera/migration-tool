

using BDO;
using BDO.GWT.Core;
using BDO.GWT.Web.Controllers;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterWebPageRoute(HomePage.CONTENT_TYPE_NAME, typeof(HomeController))]


namespace BDO.GWT.Web.Controllers;

public class HomeController : Controller
{
    public readonly IHomePageService _homePageService;

    public HomeController(IHomePageService homePageService)
    {
        _homePageService = homePageService;
    }
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var homeDto = await _homePageService.GetHomePageDtoAsync(null, cancellationToken);

        return View(homeDto);

    }
}
