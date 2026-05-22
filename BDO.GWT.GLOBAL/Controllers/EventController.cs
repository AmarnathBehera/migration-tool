
//using Microsoft.AspNetCore.Mvc;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Web.Core.LiveSite.Controllers;

//public class EventController : Controller
//{
//    private readonly IEventWebinarPageService _eventWebinarPageService;
//    private readonly IPageContext<Event> _eventPageContext;

//    public EventController(IEventWebinarPageService eventWebinarPageService, IPageContext<Event> eventPageContext)
//    {
//        _eventPageContext = eventPageContext;
//        _eventWebinarPageService = eventWebinarPageService;
//    }

//    public async Task<IActionResult> Index(CancellationToken cancellationToken)
//    {
//        var page = _eventPageContext.CurrentPage;
//        var pageDto = await _eventWebinarPageService.GetEventPageDtoAsync(page, cancellationToken);
//        var eventSchemaMarkupDto = await _eventWebinarPageService.GetEventSchemaMarkupDto(page, cancellationToken);

//        return View("~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto));
//    }
//}