namespace GWTXbyKWeb.Controllers;

using BDO.GWT.Core.Bases;
using BDO.GWT.Core.Services;
using global::BDO;

using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;


public class SectionRssController : PageController
{
    private readonly IRssSectionPageService _rssSectionPageService;

    public SectionRssController(IRssSectionPageService rssSectionPageService)
    {
        _rssSectionPageService = rssSectionPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dto = await _rssSectionPageService.GetRssSectionDtoAsync( cancellationToken);

        return View(dto);
    }
}


