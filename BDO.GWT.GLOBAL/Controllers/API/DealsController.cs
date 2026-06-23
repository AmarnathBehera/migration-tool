using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers.API
{
    [ApiController]
    [Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly ISectionDealsPageService _sectionDealsPageService;

        public DealsController(ISectionDealsPageService sectionDealsPageService)
        {
            _sectionDealsPageService = sectionDealsPageService;
        }

        [HttpGet("{pageGuid:guid}/[action]")]
        public async Task<ActionResult<PaginationDto<DealCardDto>>> Get([FromRoute] Guid pageGuid, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
        {
            query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

            var dto = await _sectionDealsPageService.GetSectionDealsDealPaginationDtoAsync(pageGuid, query, cancellationToken);

            return Ok(dto);
        }
    }
}
