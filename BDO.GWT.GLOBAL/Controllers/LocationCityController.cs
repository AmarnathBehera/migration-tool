using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class LocationCityController : Controller
{
    private readonly ILocationCityPageService _locationCityPageService;
    private readonly IPageContext _locationCityPageContext;

    public LocationCityController(ILocationCityPageService locationCityPageService, IPageContext locationCityPageContext)
    {
        _locationCityPageService = locationCityPageService;
        _locationCityPageContext = locationCityPageContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _locationCityPageContext.GetCurrentPageAsync<LocationCity>(cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _locationCityPageService.GetLocationCityDtoAsync(currentPage, cancellationToken);
        var locationCitySchemaMarkupDto = await _locationCityPageService.GetLocationCitySchemaMarkupDto(currentPage, cancellationToken);

        return View((pageDto, locationCitySchemaMarkupDto));
    }
}
