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

        public SearchController(ISearchPageService searchService)
        {
            _searchPageService = searchService;
        }

        [HttpGet]
        public async Task<SearchResultDto> Get([FromQuery] SearchResultQuery query, CancellationToken cancellationToken)
        {
            var results = await _searchPageService.GetSearchResultsAsync(query, cancellationToken);

            return results;
        }
    }
}
