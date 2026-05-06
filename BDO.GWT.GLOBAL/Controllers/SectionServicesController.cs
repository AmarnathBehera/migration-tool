using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class SectionServicesController : Controller
    {
        private readonly ISectionServicesService _sectionServicesService;

        public SectionServicesController(ISectionServicesService sectionServicesService)
        {
            _sectionServicesService = sectionServicesService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _sectionServicesService.GetSectionServicesDtoAsync(cancellationToken);
            var json = JsonConvert.SerializeObject(pageDto, Formatting.Indented);

            return View("~/Views/SectionServices/Index.cshtml", pageDto);
        }
    }
}