using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class GlobalLocationsController : Controller
{
    private readonly IContentService _contentService;
    private readonly IGlobalLocationsPageService _globalLocationsPageService;

    public GlobalLocationsController(IContentService contentService, IGlobalLocationsPageService globalLocationsPageService)
    {
        _contentService = contentService;
        _globalLocationsPageService = globalLocationsPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<GlobalLocations>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (page is null)
            return NotFound();

        var pageDto = await _globalLocationsPageService.GetGlobalLocationsPageDtoAsync(page, cancellationToken);

        return View(pageDto);
    }
}