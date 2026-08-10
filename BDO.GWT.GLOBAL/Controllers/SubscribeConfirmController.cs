using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Web.Core.LiveSite.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class SubscribeConfirmController : Controller
{
    private readonly IContentService _contentRetriever;
    private readonly ISubscribeConfirmPageService _subscribeConfirmPageService;

    public SubscribeConfirmController(IContentService contentRetriever, ISubscribeConfirmPageService subscribeConfirmPageService)
    {
        _contentRetriever = contentRetriever ?? throw new ArgumentNullException(nameof(contentRetriever));
        _subscribeConfirmPageService = subscribeConfirmPageService ?? throw new ArgumentNullException(nameof(subscribeConfirmPageService));
    }

    public async Task<IActionResult> Index([FromQuery] SubscribeConfirmQuery subscribeConfirmQuery, CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<SubscribeConfirm>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        var dto = await _subscribeConfirmPageService.GetSubscribeConfirmDtoAsync(page, subscribeConfirmQuery, cancellationToken);
        return View(dto);
    }
}
