using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class MicrositeMasterPageController : PageController
{
    private readonly IMicrositeMasterPageService _micrositeMasterPageService;

    public MicrositeMasterPageController(IMicrositeMasterPageService micrositeMasterPageService)
    {
        _micrositeMasterPageService = micrositeMasterPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dto = await _micrositeMasterPageService.GetMicrositeMasterPageDtoAsync(cancellationToken);
        return View(dto);
    }
}