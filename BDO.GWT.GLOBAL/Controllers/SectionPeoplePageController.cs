using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class SectionPeoplePageController : Controller
{
    private readonly IPageContext _context;
    private readonly ISectionPeoplePageService _sectionPeoplePageService;

    public SectionPeoplePageController(IPageContext context,ISectionPeoplePageService sectionPeoplePageService)
    {
        _context = context;
        _sectionPeoplePageService = sectionPeoplePageService;
    }
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _context.GetCurrentPageAsync<SectionPeoplePage>(cancellationToken);

        if (page is null)
            return NotFound();
        var pageDto = await _sectionPeoplePageService.GetSectionPeoplePageDtoAsync(page, cancellationToken);

        return View(pageDto);
    }
}
