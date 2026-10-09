using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

public class PersonController : Controller
{
    private readonly IContentService _contentService;
    private readonly IPersonPageService _personPageService;

    public PersonController(IContentService contentService, IPersonPageService personPageService)
    {
        _contentService = contentService;
        _personPageService = personPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<Person>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if (page is null)
            return NotFound();

        var pageDto = await _personPageService.GetPersonPageDtoAsync(page, cancellationToken);
        var personSchemaMarkupDto = await _personPageService.GetPersonSchemaMarkupDto(page, cancellationToken);

        return View((pageDto, personSchemaMarkupDto));
    }
}
