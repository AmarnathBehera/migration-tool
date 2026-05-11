using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class ServiceAreaController : Controller
    {
        private readonly IServiceAreaAndServicePageService _serviceAreaAndServicePageService;

        public ServiceAreaController(IServiceAreaAndServicePageService serviceAreaAndServicePageService)
        {
            _serviceAreaAndServicePageService = serviceAreaAndServicePageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _serviceAreaAndServicePageService.GetServiceAreaPageDtoAsync(cancellationToken);


            return View("~/Views/ServiceArea/Index.cshtml", pageDto);
        }
    }
}
