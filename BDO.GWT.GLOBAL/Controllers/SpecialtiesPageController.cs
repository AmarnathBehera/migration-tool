
using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers;

public  class SpecialtiesPageController : Controller
{

    private readonly ISpecialtiesPageService _specialtiesPageService;


    public SpecialtiesPageController(ISpecialtiesPageService specialtiesPageService)
    {
        _specialtiesPageService = specialtiesPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        
        var dto = await _specialtiesPageService.GetSpecialtiesPageDtoAsync(cancellationToken);

        return View("~/Views/SpecialtiesPage/Index.cshtml", dto);
    }
}

