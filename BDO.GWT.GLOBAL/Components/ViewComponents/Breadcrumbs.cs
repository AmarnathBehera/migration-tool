using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.ViewComponents;

public class Breadcrumbs : ViewComponent
{
    private readonly IBreadcrumbsService _breadcrumbsService;

    public Breadcrumbs(IBreadcrumbsService breadcrumbsService)
    {
        _breadcrumbsService = breadcrumbsService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var BreadCrumb = await _breadcrumbsService.GetAsync(cancellationToken);

        return View(BreadCrumb);
    }
}
