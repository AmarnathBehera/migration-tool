using BDO.GWT.Core.LiveSite.Filters;
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
        private readonly ICultureContext _cultureContext;

        public SearchController(ISearchPageService searchPageService, ICultureContext cultureContext)
        {
            _searchPageService = searchPageService;
            _cultureContext = cultureContext;
        }
        public async Task<IActionResult> Index([FromQuery] string searchText, CancellationToken cancellationToken)
        {
            var currentLanguage = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();
            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = _cultureContext.CurrentLanguageTag;
            }
            if (string.IsNullOrEmpty(searchText))
            {
                searchText = string.Empty;
            }
            var dto = await _searchPageService.GetSearchPageDtoAsync(searchText, currentLanguage, cancellationToken);
            return View(dto);
        }
    }
}
