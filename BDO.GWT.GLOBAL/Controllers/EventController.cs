
using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Services;
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
    private readonly IWebsiteChannelContext _websiteChannelContext;


    public EventController(IEventWebinarPageService eventWebinarPageService, IWebsiteChannelContext websiteChannelContext, IContentService contentRetriever)
    {
        _eventWebinarPageService = eventWebinarPageService;
        _websiteChannelContext = websiteChannelContext;
        _contentRetriever = contentRetriever;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<Event>(
          new RetrievePagesParameters
          {
              LinkedItemsMaxLevel = 5
          },
          query =>
          {
              query.Where(w =>
                  w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
          },
          RetrievalCacheSettings.CacheDisabled,
          cancellationToken);
        var EventPage = results?.FirstOrDefault();

        var pageDto = await _eventWebinarPageService.GetEventPageDtoAsync(EventPage, cancellationToken);
        var eventSchemaMarkupDto = await _eventWebinarPageService.GetEventSchemaMarkupDto(EventPage, cancellationToken);

        return View("~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto));
    }
}