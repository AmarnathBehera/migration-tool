using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Bases;
using BDO.GWT.GLOBAL.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class MicrositePageController : PageController
{
    private readonly IContentService _contentService;
    private readonly IMicrositePageService _micrositePageService;

    public MicrositePageController(IContentService contentService, IMicrositePageService micrositePageService)
    {
        _contentService = contentService;
        _micrositePageService = micrositePageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<MicrositePage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (currentPage is null)
            return NotFound();

        var dto = await _micrositePageService.GetMicrositePageDtoAsync(currentPage, cancellationToken);

        return View("~/Views/MicrositePage/Index.cshtml", dto);
    }
}
