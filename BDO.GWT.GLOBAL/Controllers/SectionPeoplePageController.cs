using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class SectionPeoplePageController : Controller
{
    private readonly IContentService _contentService;
    private readonly ISectionPeoplePageService _sectionPeoplePageService;

    public SectionPeoplePageController(IContentService contentService, ISectionPeoplePageService sectionPeoplePageService)
    {
        _contentService = contentService;
        _sectionPeoplePageService = sectionPeoplePageService;
    }
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<SectionPeoplePage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (page is null)
            return NotFound();
        var pageDto = await _sectionPeoplePageService.GetSectionPeoplePageDtoAsync(page, cancellationToken);

        return View(pageDto);
    }
}
