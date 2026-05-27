using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;
public class NewsGroupController : Controller
{
    private readonly IContentService _contentService;
    private readonly INewsGroupPageService _newsGroupPageService;

    public NewsGroupController(IContentService contentService, INewsGroupPageService newsGroupPageService)
    {
        _contentService = contentService;
        _newsGroupPageService = newsGroupPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<NewsGroup>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();
        var pageDto = await _newsGroupPageService.GetNewsGroupDtoAsync(currentPage, cancellationToken);

        return View(pageDto);
    }
}
