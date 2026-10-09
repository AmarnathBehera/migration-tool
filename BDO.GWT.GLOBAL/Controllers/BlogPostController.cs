using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly IBlogPostPageService _blogPostPageService;
        private readonly IContentService _contentService;

        public BlogPostController(IBlogPostPageService blogPostPageService, IContentService contentService)
        {
            _blogPostPageService = blogPostPageService;
            _contentService = contentService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<BlogPost>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

            if (currentPage is null)
                return NotFound();

            var pageDto = await _blogPostPageService.GetBlogPostDtoAsync(currentPage, cancellationToken);

            return View("~/Views/BlogPost/Index.cshtml", pageDto);
        }
    }
}
