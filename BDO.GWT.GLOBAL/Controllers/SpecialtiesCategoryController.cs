using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class SpecialtiesCategoryController : Controller
    {

        private readonly ISpecialtiesCategoryPageService _specialtiesCategoryPageService;

        public SpecialtiesCategoryController(ISpecialtiesCategoryPageService specialtiesCategoryPageService)
        {

            _specialtiesCategoryPageService = specialtiesCategoryPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dto = await _specialtiesCategoryPageService.GetSpecialtiesCategoryPageDtoAsync(cancellationToken);

            return View("~/Views/SpecialtiesCategory/Index.cshtml", dto);
        }
    }
}
