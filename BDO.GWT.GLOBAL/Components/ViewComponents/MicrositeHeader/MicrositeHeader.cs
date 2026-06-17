using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Global.Components.ViewComponents;

public class MicrositeHeader : ViewComponent
{
    private readonly IMicrositeHeaderService _micrositeHeaderService;
    private readonly IContentService _contentService;

    public MicrositeHeader(IMicrositeHeaderService micrositeHeaderService, IContentService contentService)
    {
        _micrositeHeaderService = micrositeHeaderService;
        _contentService = contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var headerDto = await _micrositeHeaderService.GetAsync(_contentService, cancellationToken);

        return View(headerDto);
    }
}
