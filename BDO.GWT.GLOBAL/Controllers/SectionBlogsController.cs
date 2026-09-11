using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class SectionBlogsController : Controller
    {
        private readonly ISectionBlogService _sectionBlogService;

        public SectionBlogsController(ISectionBlogService sectionBlogService)
        {
            _sectionBlogService = sectionBlogService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {

            var pageDto = await _sectionBlogService.GetSectionBlogsDtoAsync(cancellationToken);


            return View("~/Views/SectionBlogs/Index.cshtml", pageDto);
        }
    }
}
