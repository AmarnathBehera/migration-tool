using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class IndustryCategoryController : Controller
    {
        private readonly IIndustryCategoryPageService _industryCategoryPageService;

        public IndustryCategoryController(IIndustryCategoryPageService industryCategoryPageService)
        {
            _industryCategoryPageService = industryCategoryPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _industryCategoryPageService.GetIndustryCategoryPageDtoAsync(cancellationToken);
            return View("~/Views/IndustryCategory/Index.cshtml", pageDto); 
        }
    }
}