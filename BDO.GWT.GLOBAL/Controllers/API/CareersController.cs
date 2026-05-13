using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers.API;

[ApiController]
[Route($"api/{{culture}}/[controller]")]
public class CareersController: ControllerBase
    {
        private readonly ICareersPageService _careersPageService;

    public CareersController(ICareersPageService careersPageService)
    {
        _careersPageService = careersPageService;
    }

    [HttpGet("{pageGuid:int}/[action]")]
    public async Task<ActionResult<PaginationDto<ContentPageCardDto>>> Get([FromRoute] int pageGuid, CancellationToken cancellationToken)
    {
        //query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        var result = await _careersPageService.GetCareerPaginationResultAsync(pageGuid, cancellationToken);

        return Ok(result);
    }
}

