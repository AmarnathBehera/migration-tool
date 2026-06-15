using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class BlogsController : Controller
    {
        private readonly IContentService _contentService;
        private readonly IBlogsPageService _blogPageService;

        public BlogsController(IContentService contentService, IBlogsPageService blogsPageService)
        {
            _contentService = contentService;
            _blogPageService = blogsPageService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<Blog>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

            if (currentPage is null)
                return NotFound();
            var model = await _blogPageService.GetBlogsPageDtoAsync(currentPage, cancellationToken);

            return View(model);
        }
    }
}
