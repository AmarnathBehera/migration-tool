using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class SectionSpecialtiesController : Controller
{

    private readonly ISectionSpecialtiesService _sectionSpecialtiesService;

    public SectionSpecialtiesController( ISectionSpecialtiesService sectionSpecialtiesService)
    {

        _sectionSpecialtiesService = sectionSpecialtiesService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {

        var pageDto = await _sectionSpecialtiesService.GetSectionSpecialtiesDtoAsync(cancellationToken);

        return View(pageDto);
    }
}
