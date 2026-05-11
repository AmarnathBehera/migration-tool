using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers;

[ApiController]
[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly ISectionPeoplePageService _sectionPeoplePageService;

    public PeopleController(ISectionPeoplePageService sectionPeoplePageService)
    {
        _sectionPeoplePageService = sectionPeoplePageService;
    }

    [HttpGet("{pageGuid:guid}/[action]")]
    public async Task<ActionResult<PaginationDto<PersonCardDto>>> Get([FromRoute] Guid pageGuid, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        var dto = await _sectionPeoplePageService.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, query, cancellationToken);

        return Ok(dto);
    }
}
