using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route("[controller]")]
public class OutlookEventController : Controller
{
    private readonly IEventWebinarPageService _eventWebinarPageService;

    public OutlookEventController(IEventWebinarPageService eventWebinarPageService)
    {
        _eventWebinarPageService = eventWebinarPageService;
    }

    [HttpGet("{type}/{pageGuid:GUID}")]
    public async Task<IActionResult> DownloadOutlookEvent([FromRoute] Guid pageGuid, [FromRoute] string type, CancellationToken cancellationToken)
    {
        var source = Url.Action(nameof(DownloadOutlookEvent), "OutlookEvent", pageGuid, Request.Scheme);

        string? card;

        switch (type.ToLower())
        {
            case "event":
                card = await _eventWebinarPageService.GenerateOutlookEventCardContent(pageGuid, source, cancellationToken);
                break;
            case "webinar":
                card = await _eventWebinarPageService.GenerateOutlookWebinarCardContent(pageGuid, source, cancellationToken);
                break;
            default:
                throw new ArgumentException($"{type} type not found");
        }

        if (card is null)
        {
            return NotFound();
        }

        var cardBinary = Encoding.Default.GetBytes(card);
        return File(cardBinary, MediaTypeNames.Application.Octet, $"Event.ics");
    }
}
