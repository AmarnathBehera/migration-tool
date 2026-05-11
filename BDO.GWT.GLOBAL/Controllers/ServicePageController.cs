using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class ServicePageController : Controller
    {
        private readonly IServiceAreaAndServicePageService _serviceAreaAndServicePageService;

        public ServicePageController(IServiceAreaAndServicePageService serviceAreaAndServicePageService)
        {
            _serviceAreaAndServicePageService = serviceAreaAndServicePageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _serviceAreaAndServicePageService.GetServicePageDtoAsync(cancellationToken);

            return View("~/Views/ServicePage/Index.cshtml", pageDto);
        }
    }
}