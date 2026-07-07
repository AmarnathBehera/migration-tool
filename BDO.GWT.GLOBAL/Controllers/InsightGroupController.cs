using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.LiveSite.Controllers;

public class InsightGroupController :Controller
{
    private readonly IInsightGroupCardListingPageService _insightCardListingPageService;

    public InsightGroupController(IInsightGroupCardListingPageService insightCardListingPageService)
    {
        _insightCardListingPageService = insightCardListingPageService;
    }
    public async Task<IActionResult> Index(InsightGroup insightGroupPage, CancellationToken cancellationToken)
    {
        var insightPageDto = await _insightCardListingPageService.GetInsightPageDtoAsync(insightGroupPage, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }
  
}
