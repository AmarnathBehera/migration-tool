using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.LiveSite.Controllers;

public class InsightGroupController :Controller
{
    private readonly IInsightGroupCardListingPageService _insightCardListingPageService;
    private readonly IContentRetriever _contentRetriever;

    public InsightGroupController(IInsightGroupCardListingPageService insightCardListingPageService, IContentRetriever contentRetriever)
    {
        _insightCardListingPageService = insightCardListingPageService;
        _contentRetriever = contentRetriever;
    }
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var insightGroupPage = await _contentRetriever.RetrieveCurrentPage<InsightGroup>(
           new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
           _ => { },
           RetrievalCacheSettings.CacheDisabled,
           (_, currentPage) => Task.FromResult(currentPage),
           cancellationToken);
        if (insightGroupPage is null)
        {
            throw new InvalidOperationException("Current page of type InsightCategory could not be retrieved.");
        }
        var insightPageDto = await _insightCardListingPageService.GetInsightPageDtoAsync(insightGroupPage, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }
  
}
