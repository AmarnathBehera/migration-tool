using BDO.GWT.Core;
using BDO.GWT.Web.Core.Services;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class EventController : Controller
{
    private readonly IEventWebinarPageService _eventWebinarPageService;
    private readonly IContentService _contentRetriever;


    public EventController(IEventWebinarPageService eventWebinarPageService, IContentService contentRetriever)
    {
        _eventWebinarPageService = eventWebinarPageService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
     {
        var EventPage =  await _contentRetriever.RetrieveCurrentPageAsync<Event>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
        if (EventPage is null)
            return NotFound();

        var pageDto = await _eventWebinarPageService.GetEventPageDtoAsync(EventPage, cancellationToken);
        var eventSchemaMarkupDto = await _eventWebinarPageService.GetEventSchemaMarkupDto(EventPage, cancellationToken);

        return View("~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto));
    }
}