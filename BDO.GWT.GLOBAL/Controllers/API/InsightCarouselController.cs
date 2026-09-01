using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class InsightCarouselController : ControllerBase
{
    private readonly IInsightCarouselService _insightCarouselService;

    public InsightCarouselController(IInsightCarouselService insightCarouselService)
    {
        _insightCarouselService = insightCarouselService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<ContentPageCardDto>>> Items([FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        var language = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();

        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);
        var items = await _insightCarouselService.GetAPIFilteredItemsAsync(query, language, cancellationToken);

            return Ok(items);
        
    }
}
