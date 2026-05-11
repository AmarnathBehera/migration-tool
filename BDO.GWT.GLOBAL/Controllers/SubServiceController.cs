using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class SubServiceController : Controller
    {
        private readonly ISubServicesPageService _subServicesPageService;

        public SubServiceController(ISubServicesPageService subServicesPageService)
        {
            _subServicesPageService = subServicesPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _subServicesPageService.GetSubServicesPageDtoAsync(cancellationToken);
           

            return View("~/Views/SubServices/Index.cshtml", pageDto);
        }
    }
}
