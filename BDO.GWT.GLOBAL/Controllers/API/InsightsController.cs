
using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Controllers.API
{
    [Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
    public class InsightsController : ControllerBase
    {
        private readonly IInsightSectionCardListingPageService _insightCardListingPageService;

        public InsightsController(IInsightSectionCardListingPageService insightCardListingPageService)
        {
            _insightCardListingPageService = insightCardListingPageService ?? throw new ArgumentNullException(nameof(insightCardListingPageService));
        }

        [HttpGet("{pageGuid:Guid}/[action]")]
        public async Task<ActionResult<PaginationDto<ContentPageCardDto>>> Get([FromRoute] Guid pageGuid, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
        {
            query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

            var result = await _insightCardListingPageService.GetContentPaginationResultAsync(pageGuid, query, cancellationToken);

            return Ok(result);
        }
    }
}
