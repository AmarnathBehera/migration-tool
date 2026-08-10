using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class SubscribeController : Controller
{
    private readonly ISubscribePageService _subscribePageService; 
    private readonly IContentService _contentRetriever;

    public SubscribeController(ISubscribePageService subscribePageService, IContentService contentRetriever)
    {
        _subscribePageService = subscribePageService ?? throw new ArgumentNullException(nameof(subscribePageService));
        _contentRetriever = contentRetriever ?? throw new ArgumentNullException(nameof(contentRetriever));
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<Subscribe>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        var dto = await _subscribePageService.GetSubscribeDtoAsync(page, cancellationToken);
        return View(dto);
    }
}
