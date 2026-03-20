using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Footer;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Global.Components.ViewComponents;

public class Footer : ViewComponent
{
    private readonly IFooterService _footerService;

    public Footer(IFooterService footerService)
    {
        _footerService = footerService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var footer = await _footerService.GetAsync(cancellationToken);

        return View(footer);
    }
}
