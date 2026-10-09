using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class NewsArticleController : Controller
{
    private readonly IContentService _contentService;
    private readonly INewsArticleService _newsArticleService;

    public NewsArticleController(IContentService contentService, INewsArticleService newsArticleService)
    {
        _contentService = contentService;
        _newsArticleService = newsArticleService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<NewsArticle>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (currentPage is null)
            return NotFound();

        var newsPageDto = await _newsArticleService.GetSectionNewsPageDtoAsync(currentPage, cancellationToken);

        return View("~/Views/NewsArticle/Index.cshtml", newsPageDto);
    }
}
