using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers.API;

[ApiController]
[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class CareersController: ControllerBase
    {
        private readonly ICareersPageService _careersPageService;

    public CareersController(ICareersPageService careersPageService)
    {
        _careersPageService = careersPageService;
    }

    [HttpGet("{pageGuid:guid}/[action]")]
    public async Task<ActionResult<PaginationDto<ContentPageCardDto>>> Get([FromRoute] Guid pageGuid, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        var language = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();

        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        var result = await _careersPageService.GetCareerPaginationResultAsync(language, pageGuid, query, cancellationToken);

        return Ok(result);
    }
}

