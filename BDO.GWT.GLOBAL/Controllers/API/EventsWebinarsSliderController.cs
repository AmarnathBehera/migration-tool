using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Controllers.API;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class EventsWebinarsSliderController : ControllerBase
{
    private readonly IEventsWebinarsSliderService _eventsWebinarsSliderService;

    public EventsWebinarsSliderController(IEventsWebinarsSliderService eventsWebinarsSliderService)
    {
        _eventsWebinarsSliderService = eventsWebinarsSliderService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<EventsWebinarsCardDto>>> Items([FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        query.Filters.Add(FilterParameterName.ShowOngoingEvents, bool.TrueString);
        var items = await _eventsWebinarsSliderService.GetAPIFilteredItemsAsync(query, cancellationToken);


        return Ok(items);


    }

}