using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly IBlogPostPageService _blogPostPageService;

        public BlogPostController(IBlogPostPageService blogPostPageService)
        {
            _blogPostPageService = blogPostPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _blogPostPageService.GetBlogPostDtoAsync(cancellationToken);

            return View("~/Views/BlogPost/Index.cshtml", pageDto);
        }
    }
}
