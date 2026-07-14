using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class BusinessStrategyController : Controller
{
    private readonly IContentService _contentService;
    private readonly IBusinessStrategyService _businessStrategyService;

    public BusinessStrategyController(IContentService contentService,IBusinessStrategyService businessStrategyService)
    {
        _contentService=contentService;
        _businessStrategyService = businessStrategyService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<BusinessStrategy>(
           new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _businessStrategyService.GetBusinessStrategyDtoAsync(currentPage, cancellationToken);

        return View("~/Views/BusinessStrategy/Index.cshtml", pageDto);
    }
}
