using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class InsightSectionController : Controller
{
    private readonly IInsightSectionCardListingPageService _insightCardListingPageService;
    private readonly IContentRetriever _contentRetriever;

    public InsightSectionController(IInsightSectionCardListingPageService insightCardListingPageService, IContentRetriever contentRetriever)
    {
        _insightCardListingPageService = insightCardListingPageService;
        _contentRetriever = contentRetriever;
    }  
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var sectioninsights = await _contentRetriever.RetrieveCurrentPage<SectionInsightsPage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            _ => { },
            RetrievalCacheSettings.CacheDisabled,
            (_, currentPage) => Task.FromResult(currentPage),
            cancellationToken);
        if (sectioninsights is null)
        {
            throw new InvalidOperationException("Current page of type InsightCategory could not be retrieved.");
        }
        var insightPageDto = await _insightCardListingPageService.GetContentSectionDtoAsync(sectioninsights, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }

}
