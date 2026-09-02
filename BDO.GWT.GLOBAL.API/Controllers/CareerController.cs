using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    /// <summary>
    /// Provides methods to read and manage careers.
    /// </summary>
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/Careers")]
    public class CareerController : EntityControllerBase
    {
        private readonly ICareerCrudService _careerCrudService;

        public CareerController(ICareerCrudService careerCrudService)
        {
            _careerCrudService = careerCrudService;
        }

        /// <summary>
        /// Returns a single career having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
        /// <param name="pageGuid">Page GUID of a career page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns one career page.</returns>
        [HttpGet("{pageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<CareerPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<CareerPageContent>>> Get(int memberFirmID, Guid pageGuid, [FromQuery][Required] string culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _careerCrudService.GetCareerPageAsync(channelId, pageGuid, culture, cancellationToken);

            return Ok(page.Value);
        }

        /// <summary>
        /// Returns all career pages.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the pages should be retrieved.</param>
        /// <param name="culture">If specified, pages will be retrieved only of this culture. Otherwise pages of all cultures will be retrieved.</param>
        /// <param name="onlyPublished">If false, both published and unpublished pages will be retrieved. Otherwise only published pages will be retrieved.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a list of career pages.</returns>
        [HttpGet]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageOutput<CareerPageContent>>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<PageOutput<CareerPageContent>>>> Get(int memberFirmID, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pages = await _careerCrudService.GetCareerPagesAsync(channelId, culture, onlyPublished, cancellationToken);

            return Ok(pages.Value);
        }

        /// <summary>
        /// Creates a new Career page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="webPageParentID">Node ID of the page that will contain the created Career page.
        /// <br/><br/>This value can be taken from <b>Kentico Pages module</b> by selecting the parent page. 
        /// On the right side panel click on <b>Properties tab</b>, expand <b>Information</b>. The <b>Page / Node ID</b> property will contain the required value.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Input data of the new page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Newly created Career page.</returns>
        [HttpPost]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<CareerPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<CareerPageContent>>> Post(int memberFirmID, [FromQuery][Required] int webPageParentID, [FromQuery][Required] string culture, [FromBody] CareerPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _careerCrudService.CreateCareerPageAsync(channelId, webPageParentID, culture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Creates a new culture version of a career page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="pageGuid">WebPageItemGuid is the Page GUID of the career to copy content from.</param>
        /// <param name="fromCulture">Culture of the existing page version.</param>
        /// <param name="toCulture">Culture of the new page version.</param>
        /// <param name="request">New culture version data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>New career page in another culture.</returns>
        [HttpPost("{pageGuid:guid}/CreateNewCultureVersion")]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<CareerPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<CareerPageContent>>> CreateNewCultureVersion(int memberFirmID, Guid pageGuid, [FromQuery][Required] string fromCulture, [FromQuery][Required] string toCulture, [FromBody] CareerPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _careerCrudService.CreateCareerPageCultureVersionAsync(channelId, pageGuid, fromCulture, toCulture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Updates a single career page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be modified.</param>
        /// <param name="pageGuid">Page GUID of the career page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Updated career page data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated career page.</returns>
        [HttpPut("{pageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<CareerPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<CareerPageContent>>> Put(int memberFirmID, Guid pageGuid, [FromQuery][Required] string culture, [FromBody] CareerPageContent    request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _careerCrudService.UpdateCareerPageAsync(channelId, pageGuid, culture, request, cancellationToken);

            return Ok(pageDto.Value);
        }

        /// <summary>
        /// Deletes a single career page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be deleted.</param>
        /// <param name="pageGuid">Page GUID of the career page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="deleteAllCultures">Should all culture versions be deleted of this career page?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>204 No Content.</returns>
        [HttpDelete("{pageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Careers, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
        public async Task<IActionResult> Delete(int memberFirmID, Guid pageGuid, [FromQuery][Required] string culture, [FromQuery] bool deleteAllCultures = false, CancellationToken cancellationToken = default)
        {
            try
            {
                int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
                await _careerCrudService.DeleteCareerPageAsync(channelId, pageGuid, culture, deleteAllCultures, cancellationToken);

                return Ok("Successfully deleted.");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnprocessableEntityException)
            {
                return UnprocessableEntity("One or more validation errors occurred.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
