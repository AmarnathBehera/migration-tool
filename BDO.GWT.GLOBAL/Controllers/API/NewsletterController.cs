using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route("api/[controller]")]
public class NewsletterController : Controller
{
    private readonly ISubscribeFormService _subscribeFormService;

    public NewsletterController(ISubscribeFormService subscribeFormService)
    {
        _subscribeFormService = subscribeFormService;
    }

    [ValidateAntiForgeryToken]
    [HttpPost("[action]/{pageGuid:guid}")]
    public async Task<IActionResult> Subscribe([FromRoute] Guid pageGuid, [FromBody] NewsletterSubscriptionFormData newsletterSubscriptionFormData, CancellationToken cancellationToken)
    {
        var isSuccessful = await _subscribeFormService.SubscribeToNewslettersAsync(pageGuid, newsletterSubscriptionFormData, cancellationToken);
        return Ok(isSuccessful);
    }
}
