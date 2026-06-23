using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class BlogContributorsController : ControllerBase
{
    private readonly IBlogContributorsPageService _blogContributorsPageService;

    public BlogContributorsController(IBlogContributorsPageService blogContributorsPagePageService)
    {
        _blogContributorsPageService = blogContributorsPagePageService;
    }

    [HttpGet("{pageGuid:guid}/[action]")]
    public async Task<ActionResult<PaginationDto<PersonCardDto>>> Get([FromRoute] Guid pageGuid, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
    {
        var dto = await _blogContributorsPageService.GeBlogContributorsPaginationDtoAsync(pageGuid, query, cancellationToken);

        return Ok(dto);
    }
}
