using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class InsightController : Controller
{
    private readonly IInsightPageService _insightPageService;

    public InsightController(IInsightPageService insightPageService)
    {
        _insightPageService = insightPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var insightPageDto = await _insightPageService.GetInsightPageDtoAsync(null, cancellationToken);

        return View("~/Views/InsightPage/Index.cshtml", insightPageDto);
    }
}
