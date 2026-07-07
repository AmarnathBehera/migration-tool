using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Controllers;

public class InsightCategoryController : Controller
{
    private readonly IInsightCategoryCardListingPageService _insightCardListingPageService;

    public InsightCategoryController(IInsightCategoryCardListingPageService insightCardListingPageService)
    {
        _insightCardListingPageService = insightCardListingPageService;
    }
    public async Task<IActionResult> Index(InsightCategory insightCPage, CancellationToken cancellationToken)
    {
        var insightPageDto = await _insightCardListingPageService.GetInsightPageDtoAsync(insightCPage, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }
}
