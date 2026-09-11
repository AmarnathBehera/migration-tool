using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

[ApiController]
[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class NewsCarouselController : ControllerBase
{
    private readonly INewsCarouselService _newsCarouselService;

    public NewsCarouselController(INewsCarouselService newsCarouselService)
    {
        _newsCarouselService = newsCarouselService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<NewsPageCardDto>>> Items(
        [FromQuery] CardsQuery query,
        CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);
        var result = await _newsCarouselService.GetAPIFilteredItemsAsync(query, cancellationToken);
        return Ok(result);
    }
}