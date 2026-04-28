using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class LocationsController : Controller
{
    private readonly ILocationsPageService _locationsPageService;
    private readonly IPageContext _locationsPageContext;

    public LocationsController(ILocationsPageService locationsPageService,IPageContext locationsPageContext)
    {
        _locationsPageService = locationsPageService;
        _locationsPageContext = locationsPageContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _locationsPageContext.GetCurrentPageAsync<Locations>(cancellationToken);

        if (currentPage is null)
            return NotFound();

        var pageDto = await _locationsPageService.GetLocationsDtoAsync(currentPage, cancellationToken);
        var locationsSchemaMarkupDto = await _locationsPageService.GetLocationsSchemaMarkupDto(currentPage, cancellationToken);

        return View((pageDto, locationsSchemaMarkupDto));
    }
}
