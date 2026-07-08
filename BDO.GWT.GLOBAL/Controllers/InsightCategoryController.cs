using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Controllers;

public class InsightCategoryController : Controller
{
    private readonly IInsightCategoryCardListingPageService _insightCardListingPageService;
    private readonly IContentRetriever _contentRetriever;

    public InsightCategoryController(IInsightCategoryCardListingPageService insightCardListingPageService, IContentRetriever contentRetriever)
    {
        _insightCardListingPageService = insightCardListingPageService;
        _contentRetriever= contentRetriever;
    }
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var insightPage = await _contentRetriever.RetrieveCurrentPage<InsightCategory>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            _ => { },
            RetrievalCacheSettings.CacheDisabled,
            (_, currentPage) => Task.FromResult(currentPage),
            cancellationToken);
        if (insightPage is null)
        {
            throw new InvalidOperationException("Current page of type InsightCategory could not be retrieved.");
        }
        var insightPageDto = await _insightCardListingPageService.GetInsightPageDtoAsync(insightPage, cancellationToken);

        return View("~/Views/InsightListing/Index.cshtml", insightPageDto);
    }
}
