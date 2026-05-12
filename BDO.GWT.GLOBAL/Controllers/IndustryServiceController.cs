using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class IndustryServiceController : Controller
    {
        private readonly IIndustryServicePageService _industryServicePageService;

        public IndustryServiceController(IIndustryServicePageService industryServicePageService)
        {
            _industryServicePageService = industryServicePageService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _industryServicePageService.GetIndustryServiceDtoAsync(cancellationToken);
            return View("~/Views/IndustryService/Index.cshtml", pageDto);
        }
    }
}
