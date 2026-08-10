using BDO.GWT.Core;
using BDO.GWT.Web.Core.Data.Queries;
using BDO.GWT.Web.Core.LiveSite.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class SubscribeRemoveController : Controller
{
    private readonly IContentService _contentRetriever;
    private readonly ISubscribeRemovePageService _subscribeRemovePageService;

    public SubscribeRemoveController(ISubscribeRemovePageService subscribeRemovePageService, IContentService contentRetriever)
    {
        _subscribeRemovePageService = subscribeRemovePageService ?? throw new ArgumentNullException(nameof(subscribeRemovePageService));
        _contentRetriever = contentRetriever ?? throw new ArgumentNullException(nameof(contentRetriever));
    }

    public async Task<IActionResult> Index([FromQuery] SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<SubscribeRemove>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        var dto = await _subscribeRemovePageService.GetSubscribeRemoveDtoAsync(page, subscribeRemoveQuery, cancellationToken);
        return View(dto);
    }
}
