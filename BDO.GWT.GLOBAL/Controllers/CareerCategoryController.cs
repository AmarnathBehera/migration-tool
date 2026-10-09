using BDO.GWT.Core.Bases;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class CareerCategoryController : PageController
{
    public readonly ICareerCategoryPageService _careerCategoryPageService;

    public CareerCategoryController(ICareerCategoryPageService careerCategoryPageService)
    {
        _careerCategoryPageService = careerCategoryPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)

    {
        var pageDto = await _careerCategoryPageService.GetCareerCategoryDtoAsync(cancellationToken);

        return View(pageDto);
    }
}

