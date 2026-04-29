
using BDO;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Services;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterWebPageRoute(StatusCode.CONTENT_TYPE_NAME, typeof(ErrorController))]

namespace BDO.GWT.Core.Services;



[Route("Error/{code:int}")]
public class ErrorController : MvcController
{
    private readonly IErrorService _errorService;

    public ErrorController(IErrorService errorService)
    {
        _errorService = errorService;
    }

    public async Task<IActionResult> Index(int code, CancellationToken cancellationToken)
    {
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        var statusCodePage = await _errorService.GetStatusCodePageAsync(statusCodeReExecuteFeature, cancellationToken);

        if (statusCodePage is null)
        {
            return StatusCode(code);
        }

        return View(statusCodePage);
    }
}
