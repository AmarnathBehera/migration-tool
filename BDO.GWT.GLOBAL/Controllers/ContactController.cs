using BDO;
using BDO.GWT.Core;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers;

public class ContactController : Controller
{
    private readonly XperiencePageContext _contactPageContext;
    private readonly IContactPageService _contactPageService;

    public ContactController(XperiencePageContext contactPageContext, IContactPageService contactPageService)
    {
        _contactPageContext = contactPageContext;
        _contactPageService = contactPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _contactPageContext.GetCurrentPageAsync();
        var pageDto = await _contactPageService.GetContactPageDtoAsync(page, cancellationToken);

        return View("~/Views/FormPage/Index.cshtml", pageDto);
    }
}
