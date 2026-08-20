using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers.API
{
    [ApiController]
    [Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[Controller]")]
    public class TypeAheadController : ControllerBase
    {
        private readonly ITypeAheadSearchService _typeAheadSearchService;

        public TypeAheadController(ITypeAheadSearchService typeAheadSearchService)
        {
            _typeAheadSearchService = typeAheadSearchService;
        }

        [HttpGet("suggestions")]
        public async Task<IEnumerable<TypeAheadSearchResultDto>> GetSuggestions([FromQuery] string searchText, CancellationToken cancellationToken)
        {
            var results = await _typeAheadSearchService.GetSuggestions(searchText, cancellationToken);

            return results;
        }
    }
}
