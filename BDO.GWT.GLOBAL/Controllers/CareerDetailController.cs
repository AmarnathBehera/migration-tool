using BDO.GWT.Core.Bases;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public  class CareerDetailController : PageController 
{
    private readonly ICareerDetailPageService _careerDetailPageService;
    private readonly IPageContext _careerdetailcontext;


    public CareerDetailController(ICareerDetailPageService careerDetailPageService, IPageContext careerdetailcontext)
    {
        _careerDetailPageService = careerDetailPageService;
        _careerdetailcontext = careerdetailcontext;

    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
 
        var pageDto = await _careerDetailPageService.GetCareerPageDtoAsync(cancellationToken);

        return View(pageDto);
    }
}
