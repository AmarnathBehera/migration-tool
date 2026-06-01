using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class CareerContentController : PageController
{
    public readonly ICareerContentPageService _careerContentPageService;

    public CareerContentController(ICareerContentPageService careerContentPageService)
    {
        _careerContentPageService = careerContentPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)

    {
        var pageDto = await _careerContentPageService.GetCareerContentDtoAsync(cancellationToken);

        return View("~/Views/CareerContent/Index.cshtml", pageDto);
    }
}

