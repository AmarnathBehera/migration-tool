using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class IndustryController : Controller
    {
        private readonly IIndustryPageService _industryPageService;

        public IndustryController(IIndustryPageService industryPageService)
        {
            _industryPageService = industryPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _industryPageService.GetIndustryPageDtoAsync(cancellationToken);
            return View("~/Views/Industry/Index.cshtml", pageDto);
        }
    }
}