using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class SectionBusinessStrategiesController : Controller
{
    private readonly IContentService _contentService;
    private readonly ISectionBusinessStrategiesService _sectionBusinessStrategiesService;

    public SectionBusinessStrategiesController(IContentService contentService, ISectionBusinessStrategiesService sectionBusinessStrategiesService)
    {
        _contentService = contentService;
        _sectionBusinessStrategiesService = sectionBusinessStrategiesService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<SectionBusinessStrategies>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _sectionBusinessStrategiesService.GetBusinessStrategiesDtoAsync(currentPage, cancellationToken);

        return View("~/Views/SectionBusinessStrategies/Index.cshtml", pageDto);
    }
}
