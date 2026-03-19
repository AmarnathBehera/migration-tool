using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Global.Components.ViewComponents;

public class Footer : ViewComponent
{
    private readonly IFooterService _headerService;
    private readonly PageContext _pageContext;

    public Footer(IFooterService headerService)
    {
        _headerService = headerService;

    }

    public async Task<IViewComponentResult> InvokeAsync()
    {

        return View();
    }
}
