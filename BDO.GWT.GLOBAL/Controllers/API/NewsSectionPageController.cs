using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class NewsSectionPageController : ControllerBase
{
    private readonly INewsSectionPageService _newsSectionPageService;

    public NewsSectionPageController(INewsSectionPageService newsSectionPageService)
    {
        _newsSectionPageService = newsSectionPageService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<PaginationDto<ContentPageCardDto>>> Get([FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        var result = await _newsSectionPageService.GetNewsArticlePaginationResultAsync(query, cancellationToken);

        return Ok(result);
    }
}
