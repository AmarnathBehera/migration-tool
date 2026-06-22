namespace GWTXbyKWeb.Controllers;

using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using global::BDO.GWT.Core.Bases;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;


public class SitemapPageController : PageController
{
    private readonly ISitemapPageService _sitemapPageService;

    public SitemapPageController(ISitemapPageService sitemapPageService)
    {

        _sitemapPageService = sitemapPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageDto = await _sitemapPageService.GetSitemapPageDtoAsync( cancellationToken);

        return View(pageDto);
    }
}
