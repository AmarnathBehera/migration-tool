using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers.API
{
    [ApiController]
    [Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[Controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchPageService _searchPageService;
        private readonly ICultureContext _cultureContext;
        public SearchController(ISearchPageService searchService, ICultureContext cultureContext)
        {
            _searchPageService = searchService;
            _cultureContext = cultureContext;
        }

        [HttpGet]
        public async Task<SearchResultDto> Get([FromQuery] SearchResultQuery query,  CancellationToken cancellationToken)
        {
            var currentLanguage = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();
           
            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = _cultureContext.CurrentLanguageTag;
            }
           
            var results = await _searchPageService.GetSearchResultsAsync(query, currentLanguage, cancellationToken);

            return results;
        }
    }
}
