using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class EventsSectionPageController : ControllerBase
{
    private readonly ISectionEventsService _eventsSectionPageService;

    public EventsSectionPageController(ISectionEventsService eventsSectionPageService)
    {
        _eventsSectionPageService = eventsSectionPageService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<PaginationDto<EventsWebinarsCardDto>>> Items([FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        query.Filters.Add(FilterParameterName.ShowOngoingEvents, bool.TrueString);

        var language = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();

        var items = await _eventsSectionPageService.GetFilteredItemsAsync(query, language, cancellationToken);

        return Ok(items);
    }
}


