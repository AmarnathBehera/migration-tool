using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class SectionDealsController : PageController
{
    private readonly ISectionDealsPageService _sectionDealsPageService;

    public SectionDealsController(ISectionDealsPageService sectionDealsPageService)
    {
        _sectionDealsPageService = sectionDealsPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageDto = await _sectionDealsPageService.GetSectionDealsDtoAsync(cancellationToken);

        return View(pageDto);
    }
}
