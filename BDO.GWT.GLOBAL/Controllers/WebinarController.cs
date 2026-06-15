

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
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public WebinarController(IEventWebinarPageService eventWebinarPageService, IWebsiteChannelContext websiteChannelContext, IContentService contentRetriever)
    {
        _eventWebinarPageService = eventWebinarPageService;
        _websiteChannelContext = websiteChannelContext;
        _contentRetriever = contentRetriever;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<Webinar>(
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
        var WebinarPage = results?.FirstOrDefault();

        var pageDto = await _eventWebinarPageService.GetWebinarPageDtoAsync(WebinarPage, cancellationToken);
        var eventSchemaMarkupDto = await _eventWebinarPageService.GetWebinarSchemaMarkupDto(WebinarPage, cancellationToken);


        return View("~/Views/EventWebinar/Index.cshtml", (pageDto, eventSchemaMarkupDto));
    }
}
