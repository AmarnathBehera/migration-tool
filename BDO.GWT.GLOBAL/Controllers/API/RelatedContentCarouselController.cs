using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class RelatedContentCarouselController : ControllerBase
{
    private readonly IRelatedContentCarouselService _relatedContentCarouselService;

    public RelatedContentCarouselController(IRelatedContentCarouselService relatedContentCarouselService)
    {
        _relatedContentCarouselService = relatedContentCarouselService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<ContentPageCardDto>>> Items([FromQuery] RelatedContentCardsQuery query, CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);
        var items = await _relatedContentCarouselService.GetAPIFilteredItemsAsync(query, cancellationToken);

        return Ok(items);

    }
}
