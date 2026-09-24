using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class BlogContributorsPageController : Controller
    {
        private readonly IBlogContributorsPageService _blogContributorsPageService;
        private readonly IContentService _contentService;

        public BlogContributorsPageController(IBlogContributorsPageService blogContributorsPageService, IContentService contentService)
        {
            _blogContributorsPageService = blogContributorsPageService;
            _contentService = contentService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<BlogContributors>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

            if (currentPage is null)
                return NotFound();

            var pageDto = await _blogContributorsPageService.GetSectionPeoplePageDtoAsync(currentPage, cancellationToken);

            return View("~/Views/BlogContributorsPage/Index.cshtml", pageDto);

        }
    }
}
