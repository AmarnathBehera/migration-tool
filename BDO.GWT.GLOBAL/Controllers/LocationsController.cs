using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class LocationsController : Controller
{
    private readonly IContentService _contentService;
    private readonly ILocationsPageService _locationsPageService;

    public LocationsController(IContentService contentService, ILocationsPageService locationsPageService)
    {
        _contentService = contentService;
        _locationsPageService = locationsPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<Locations>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _locationsPageService.GetLocationsDtoAsync(currentPage, cancellationToken);
        var locationsSchemaMarkupDto = await _locationsPageService.GetLocationsSchemaMarkupDto(currentPage, cancellationToken);

        return View((pageDto, locationsSchemaMarkupDto));
    }
}
