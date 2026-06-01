using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class CareerController : PageController
{
    public readonly ICareerPageService _careerPageService;

    public CareerController(ICareerPageService careerPageService)
    {
        _careerPageService = careerPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    
    {
        var pageDto = await _careerPageService.GetCareerDtoAsync(cancellationToken);

        return View("~/Views/Career/Index.cshtml", pageDto);
    }
}

