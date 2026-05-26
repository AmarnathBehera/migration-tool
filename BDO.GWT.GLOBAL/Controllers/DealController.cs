using BDO.GWT.Core.Bases;
using BDO.GWT.Domain.Core.Entities.Pages;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Bases;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class DealController : PageController
{
    private readonly IDealPageService _dealPageService;

    public DealController(IDealPageService dealPageService)
    {
        _dealPageService = dealPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageDto = await _dealPageService.GetDealPageDtoAsync(cancellationToken);

        return View(pageDto);
    }
}
