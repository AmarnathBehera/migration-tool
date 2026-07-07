using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class InsightSectionController : Controller
{
    private readonly IInsightSectionCardListingPageService _insightCardListingPageService;

    public InsightSectionController(IInsightSectionCardListingPageService insightCardListingPageService)
    {
        _insightCardListingPageService = insightCardListingPageService;
    }
    public async Task<IActionResult> Index(SectionInsightsPage sectioninsights, CancellationToken cancellationToken)
    {
        var insightPageDto = await _insightCardListingPageService.GetContentSectionDtoAsync(sectioninsights, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }

}
