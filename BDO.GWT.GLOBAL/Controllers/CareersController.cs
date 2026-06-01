using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class CareersController : PageController
{
    private readonly ICareersPageService _careersPageService;

    public CareersController(ICareersPageService careersPageService)
    {
        _careersPageService = careersPageService;

    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {

        var pageDto = await _careersPageService.GetCareersPageDtoAsync( cancellationToken);

        return View(pageDto);
    }
}
