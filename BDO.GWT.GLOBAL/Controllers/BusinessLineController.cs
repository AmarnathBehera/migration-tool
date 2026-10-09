using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class BusinessLineController : Controller
    {
        private readonly IBusinessLinePageService _businessLinePageService;

        public BusinessLineController(IBusinessLinePageService businessLinePageService)
        {
            _businessLinePageService = businessLinePageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _businessLinePageService.GetBusinessLinePageDtoAsync(cancellationToken);
            var schemaMarkupDto = await _businessLinePageService.GetServicesPrimarySchemaMarkupDto(pageDto, cancellationToken);

            return View("~/Views/BusinessLine/Index.cshtml", (pageDto, schemaMarkupDto));
        }
    }
}