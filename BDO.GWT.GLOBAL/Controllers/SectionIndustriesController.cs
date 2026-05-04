using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class SectionIndustriesController : Controller
    {
        private readonly ISectionIndustriesService _sectionIndustriesService;

        public SectionIndustriesController(ISectionIndustriesService sectionIndustriesService)
        {
            _sectionIndustriesService = sectionIndustriesService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _sectionIndustriesService.GetSectionIndustriesDtoAsync(cancellationToken);
            var json = JsonConvert.SerializeObject(pageDto, Formatting.Indented);

            return View("~/Views/SectionIndustries/Index.cshtml", pageDto);
        }
    }
}