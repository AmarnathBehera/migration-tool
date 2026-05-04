using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class LocationCityController : Controller
{
    private readonly IContentService _contentService;
    private readonly ILocationCityPageService _locationCityPageService;

    public LocationCityController(IContentService contentService, ILocationCityPageService locationCityPageService)
    {
        _contentService = contentService;
        _locationCityPageService = locationCityPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<LocationCity>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _locationCityPageService.GetLocationCityDtoAsync(currentPage, cancellationToken);
        var locationCitySchemaMarkupDto = await _locationCityPageService.GetLocationCitySchemaMarkupDto(currentPage, cancellationToken);

        return View((pageDto, locationCitySchemaMarkupDto));
    }
}
