

using BDO.GWT.Core;
using BDO.GWT.Web.Core.Services;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class WebinarController : Controller
{
    private readonly IEventWebinarPageService _eventWebinarPageService;
    private readonly IContentService _contentRetriever;

    public WebinarController(IEventWebinarPageService eventWebinarPageService, IContentService contentRetriever)
    {
        _eventWebinarPageService = eventWebinarPageService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {

        var WebinarPage = await _contentRetriever.RetrieveCurrentPageAsync<Webinar>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
        if (WebinarPage is null)
            return NotFound();


        var pageDto = await _eventWebinarPageService.GetWebinarPageDtoAsync(WebinarPage, cancellationToken);
        var eventSchemaMarkupDto = await _eventWebinarPageService.GetWebinarSchemaMarkupDto(WebinarPage, cancellationToken);


        return View("~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto));
    }
}
