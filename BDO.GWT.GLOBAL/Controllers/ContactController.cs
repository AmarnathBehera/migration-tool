using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Bases;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class ContactController : PageController
{
    private readonly IContactPageService _contactPageService;

    public ContactController(IContactPageService contactPageService)
    {
        _contactPageService = contactPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var pageDto = await _contactPageService.GetContactPageDtoAsync(null, cancellationToken);

        return View("~/Views/FormPage/Index.cshtml", pageDto);
    }
}
