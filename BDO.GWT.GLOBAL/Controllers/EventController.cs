
using BDO.GWT.Core;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class EventController : Controller
{
    //private readonly IEventWebinarPageService _eventWebinarPageService;
    private readonly IContentService _contentRetriever;

    public EventController(/*IEventWebinarPageService eventWebinarPageService,*/ IContentService contentRetriever)
    {
        //_eventWebinarPageService = eventWebinarPageService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        //var pageDto = await _eventWebinarPageService.GetEventPageDtoAsync(null, cancellationToken);
        //var eventSchemaMarkupDto = await _eventWebinarPageService.GetEventSchemaMarkupDto(null, cancellationToken);

        return View(/*"~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto)*/);
    }
}