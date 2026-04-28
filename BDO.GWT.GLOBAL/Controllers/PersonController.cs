using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class PersonController : Controller
{
    private readonly IPageContext _context;
    private readonly IPersonPageService _personPageService;

    public PersonController(IPageContext personPageContext, IPersonPageService personPageService)
    {
        _context = personPageContext;
        _personPageService = personPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _context.GetCurrentPageAsync<Person>(cancellationToken);

        if (page is null)
            return NotFound();

        var pageDto = await _personPageService.GetPersonPageDtoAsync(page, cancellationToken);
        var personSchemaMarkupDto = await _personPageService.GetPersonSchemaMarkupDto(page, cancellationToken);

        return View((pageDto, personSchemaMarkupDto));
    }
}
