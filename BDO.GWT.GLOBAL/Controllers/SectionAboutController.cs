using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class SectionAboutController : Controller
    {
        private readonly ISectionAboutPageService _sectionAboutPageService;

        public SectionAboutController(ISectionAboutPageService aboutPageService)
        {
            _sectionAboutPageService = aboutPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _sectionAboutPageService.GetAboutDtoAsync(cancellationToken);

            return View("~/Views/SectionAbout/Index.cshtml", pageDto);
        }
    }
}   