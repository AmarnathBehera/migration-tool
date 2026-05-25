using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;
public class NewsSectionController : Controller
{
    private readonly IContentService _contentService;
    private readonly INewsSectionPageService _newsSectionPageService;

    public NewsSectionController(IContentService contentService, INewsSectionPageService newsSectionPageService)
    {
        _contentService = contentService;
        _newsSectionPageService = newsSectionPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<SectionNews>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();
        var pageDto = await _newsSectionPageService.GetNewsSectionDtoAsync(currentPage, cancellationToken);

        return View(pageDto);
    }
}
