using BDO;
using BDO.GWT.Core.Bases;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public abstract class AboutController<TPage> : PageController
    where TPage : class, IHero, IOGMetadataFields, ISearchAndSEO, IMetadata, IWebPageFieldsSource, new()
{
    private readonly IPageContext _aboutContext;
    private readonly IAboutPageService<TPage> _aboutPageService;

    protected AboutController(IPageContext aboutContext, IAboutPageService<TPage> aboutPageService)
    {
        _aboutContext = aboutContext;
        _aboutPageService = aboutPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _aboutContext.GetCurrentPageAsync<TPage>(cancellationToken);

        if (page is null)
            return NotFound();

        var pageDto = await _aboutPageService.GetAboutDtoAsync(page, cancellationToken);

        return View("~/Views/About/Index.cshtml", pageDto);
    }
}