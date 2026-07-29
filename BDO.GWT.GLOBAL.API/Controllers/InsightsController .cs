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
    /// Provides methods to read and manage insights.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/Insights")]
    public class InsightsController : EntityControllerBase
    {
        private readonly IInsightCrudService _insightCrudService;

        public InsightsController(IInsightCrudService insightCrudService)
        {
            _insightCrudService = insightCrudService;
        }

        /// <summary>
        /// Returns all insight pages.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the pages should be retrieved.</param>
        /// <param name="culture">If specified, pages will be retrieved only of this culture. Otherwise pages of all cultures will be retrieved.</param>
        /// <param name="onlyPublished">If false, both published and unpublished pages will be retrieved. Otherwise only published pages will be retrieved.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a list of insights.</returns>
        [HttpGet]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageOutput<InsightPageContent>>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<PageOutput<InsightPageContent>>>> Get(int memberFirmID, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pages = await _insightCrudService.GetInsightPagesAsync(channelId, culture, onlyPublished, cancellationToken);

            return Ok(pages);
        }

        /// <summary>
        /// Returns a single insight having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
        /// <param name="PageGuid">Page GUID of an insight page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns one insight page.</returns>
        [HttpGet("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<InsightPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<InsightPageContent>>> Get(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _insightCrudService.GetInsightPageAsync(channelId, PageGuid, culture, cancellationToken);

            return Ok(page);
        }

        /// <summary>
        /// Creates a new insight page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="WebPageParentID">WebPageParent ID of the page that will contain the created insight page.
        /// <br/><br/>This value can be taken from <b>Kentico Pages module</b> by selecting the parent. Select <b>General tab</b>, then click <b>Advanced</b>. The <b>Node ID</b> property will contain the required value.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Input data of the new page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Newly created insight page.</returns>
        [HttpPost]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<InsightPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<InsightPageContent>>> Post(int memberFirmID, [FromQuery][Required] int WebPageParentID, [FromQuery][Required] string culture, [FromBody] InsightPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _insightCrudService.CreatePageAsync(channelId, WebPageParentID, culture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Creates a new culture version of an insight page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="PageGuid">Page GUID of the insight to copy content from.</param>
        /// <param name="fromCulture">Culture of the existing page version.</param>
        /// <param name="toCulture">Culture of the new page version.</param>
        /// <param name="request">New culture version data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>New insight page in another culture.</returns>
        [HttpPost("{PageGuid:guid}/CreateNewCultureVersion")]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<InsightPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<InsightPageContent>>> CreateNewCultureVersion(int memberFirmID, Guid PageGuid, [FromQuery][Required] string fromCulture, [FromQuery][Required] string toCulture, [FromBody] InsightPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _insightCrudService.CreatePageCultureVersionAsync(channelId, PageGuid, fromCulture, toCulture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Updates a single insight page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be modified.</param>
        /// <param name="PageGuid">Page GUID of the insight page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Updated insight page data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated insight page.</returns>
        [HttpPut("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<InsightPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<InsightPageContent>>> Put(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, [FromBody] InsightPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _insightCrudService.UpdatePageAsync(channelId, PageGuid, culture, request, cancellationToken);

            return Ok(pageDto.Value);
        }

        /// <summary>
        /// Deletes a single insight page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be deleted.</param>
        /// <param name="PageGuid">Page GUID of the insight.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="deleteAllCultures">Should all culture versions be deleted of this insight?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>204 No Content.</returns>
        [HttpDelete("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.Insights, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
        public async Task<IActionResult> Delete(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, [FromQuery] bool deleteAllCultures = false, CancellationToken cancellationToken = default)
        {
            try
            {
                int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
                await _insightCrudService.DeletePageAsync(channelId, PageGuid, culture, deleteAllCultures, cancellationToken);

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