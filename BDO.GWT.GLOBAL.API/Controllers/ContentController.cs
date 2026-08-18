using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("memberFirms/{memberFirmId:int}/{culture}/content")]
    public class ContentController : EntityControllerBase
    {
        private readonly IContentCategoryService _contentService;

        public ContentController(IContentCategoryService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet("categories", Name = "GetContentCategories")]
        [HasPermission(ApiMethodGroupName.Content, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<ContentCategoryDto>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategories(int memberFirmId,string? culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var dtos = await _contentService.GetContentCategoriesAsync(channelId,culture, cancellationToken);

            return Ok(dtos);
        }

        [HttpPost(Name = "GetContentFromCategories")]
        [HasPermission(ApiMethodGroupName.Content, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ContentDto), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<ContentDto>> GetContentFromCategories([FromRoute] int memberFirmId, [FromRoute] string culture, [FromQuery] int countInsights, [FromQuery] int countWhatsNew, [FromBody] ContentCategoryFilter[] categoryFilters, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var dto = await _contentService.GetContentAsync(channelId, culture, countInsights, countWhatsNew, categoryFilters, cancellationToken);

            return Ok(dto);
        }
    }
}