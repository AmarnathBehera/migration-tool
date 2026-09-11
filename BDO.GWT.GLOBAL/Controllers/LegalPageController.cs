using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class LegalPageController : Controller
    {
        private readonly ILegalPageService _legalPageService;

        public LegalPageController(ILegalPageService legalPageService)
        {
            _legalPageService = legalPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pageDto = await _legalPageService.GetLegalPageDtoAsync(cancellationToken);
            return View("~/Views/LegalPage/Index.cshtml", pageDto);
        }
    }
}
