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
        private readonly IContentService1 _contentService;

        public ContentController(IContentService1 contentService)
        {
            _contentService = contentService;
        }
        [AllowAnonymous]
        [HttpGet("categories", Name = "GetContentCategories")]
        //[HasPermission(ApiMethodGroupName.Content, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<ContentCategoryDto>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategories([FromRoute] int memberFirmId, [FromRoute] string culture, CancellationToken cancellationToken)
        {
            var dtos = await _contentService.GetContentCategoriesAsync(culture, cancellationToken);

            return Ok(dtos);
        }
    }
}