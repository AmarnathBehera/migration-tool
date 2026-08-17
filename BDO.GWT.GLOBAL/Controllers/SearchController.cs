using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchPageService _searchPageService;

        public SearchController(ISearchPageService searchPageService)
        {
            _searchPageService = searchPageService;
        }
        public async Task<IActionResult> Index([FromQuery] string? searchText, CancellationToken cancellationToken)
        {
            var dto = await _searchPageService.GetSearchPageDtoAsync(searchText ?? "", cancellationToken);
            return View(dto);
        }
    }
}
