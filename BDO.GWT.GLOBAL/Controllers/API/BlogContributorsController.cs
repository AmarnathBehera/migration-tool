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
        //getting the current language and passing it to the service method to
        //get the paginated blog contributors based on the content Item GUID, language, and query parameters
        var language = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();

        var dto = await _blogContributorsPageService.GetBlogContributorsPaginationDtoAsync(pageGuid, language, query, cancellationToken);

        return Ok(dto);
    }
}
