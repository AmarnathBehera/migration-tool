using BDO.GWT.Core;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class EventRegistrationFormController : Controller
{
    private readonly IEventRegistrationFormPageService _eventRegistrationFormPageService;

    public EventRegistrationFormController(IEventRegistrationFormPageService eventRegistrationFormPageService, IContentService contentService)
    {
        _eventRegistrationFormPageService = eventRegistrationFormPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageDto = await _eventRegistrationFormPageService.GetEventRegistrationFormPageDtoAsync(cancellationToken);

        return View("~/Views/FormPage/Index.cshtml", pageDto);
    }
}
