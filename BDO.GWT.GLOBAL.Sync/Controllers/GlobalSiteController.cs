using BDO.GWT.GLOBAL.Sync.Attributes;
using BDO.GWT.GLOBAL.Sync.Services;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.Sync.Controllers;

[ApiController]
[AuthorizeApiKey]
[Route("GlobalSite")]
public class GlobalSiteController : ControllerBase
{
    private readonly IGlobalDealsSynchronizer _globalDealsSynchronizer;

    public GlobalSiteController(IGlobalDealsSynchronizer globalDealsSynchronizer)
    {
        _globalDealsSynchronizer = globalDealsSynchronizer;
    }

    [HttpPost("SynchronizeGlobalDeals")]
    public async Task SynchronizeGlobalDeals([FromQuery] bool forceUpdate)
    {
        await _globalDealsSynchronizer.SynchronizeGlobalDealsAsync(forceUpdate, CancellationToken.None);
    }
}
