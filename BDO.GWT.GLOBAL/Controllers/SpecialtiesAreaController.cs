using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class SpecialtiesAreaController :Controller
    {

        private readonly ISpecialtiesAreaPageService _specialtiesAreaPageService;

        public SpecialtiesAreaController(ISpecialtiesAreaPageService specialtiesAreaPageService)
        {

            _specialtiesAreaPageService = specialtiesAreaPageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dto = await _specialtiesAreaPageService.GetSpecialtiesAreaPageDtoAsync(cancellationToken);

            return View("~/Views/SpecialtiesArea/Index.cshtml", dto);
        }
    }
}
