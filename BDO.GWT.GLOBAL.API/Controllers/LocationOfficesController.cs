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
    /// Provides methods to read and manage location offices.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/LocationOffices")]
    public class LocationOfficesController : EntityControllerBase
    {
        private readonly ILocationOfficeCrudService _locationOfficeCrudService;

        public LocationOfficesController(ILocationOfficeCrudService locationOfficeCrudService)
        {
            _locationOfficeCrudService = locationOfficeCrudService;
        }

        /// <summary>
        /// Returns all location office pages.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the pages should be retrieved.</param>
        /// <param name="culture">If specified, pages will be retrieved only of this culture. Otherwise pages of all cultures will be retrieved.</param>
        /// <param name="onlyPublished">If false, both published and unpublished pages will be retrieved. Otherwise only published pages will be retrieved.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a list of location offices.</returns>
        [HttpGet]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageOutput<LocationOfficePageContent>>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<PageOutput<LocationOfficePageContent>>>> Get(int memberFirmID, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pages = await _locationOfficeCrudService.GetPagesAsync(channelId, culture, onlyPublished, cancellationToken);

            return Ok(pages);
        }

        /// <summary>
        /// Returns a single location office having the specified Page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
        /// <param name="PageGuid">Page GUID of a location office page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns one location office page.</returns>
        [HttpGet("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<LocationOfficePageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> Get(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _locationOfficeCrudService.GetPageAsync(channelId, PageGuid, culture, cancellationToken);

            return Ok(page);
        }

        /// <summary>
        /// Creates a new location office page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="WebPageParentID">WebPageParentItem ID of the page that will contain the created location office page.
        /// <br/><br/>This value can be taken from <b>Kentico Pages module</b> by selecting the parent. Select <b>General tab</b>, then click <b>Advanced</b>. The <b>Node ID</b> property will contain the required value.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Input data of the new page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Newly created location office page.</returns>
        [HttpPost]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<LocationOfficePageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> Post(int memberFirmID, [FromQuery][Required] int WebPageParentID, [FromQuery][Required] string culture, [FromBody] LocationOfficePageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _locationOfficeCrudService.CreatePageAsync(channelId, WebPageParentID, culture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Creates a new culture version of a location office page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="PageGuid">Page GUID of the location office to copy content from.</param>
        /// <param name="fromCulture">Culture of the existing page version.</param>
        /// <param name="toCulture">Culture of the new page version.</param>
        /// <param name="request">New culture version data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>New location office page in another culture.</returns>
        [HttpPost("{PageGuid:guid}/CreateNewCultureVersion")]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<LocationOfficePageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> CreateNewCultureVersion(int memberFirmID, Guid PageGuid, [FromQuery][Required] string fromCulture, [FromQuery][Required] string toCulture, [FromBody] LocationOfficePageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _locationOfficeCrudService.CreatePageCultureVersionAsync(channelId, PageGuid, fromCulture, toCulture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Updates a single location office page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be modified.</param>
        /// <param name="PageGuid">Page GUID of the location office page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Updated location office page data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated location office page.</returns>
        [HttpPut("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<LocationOfficePageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<LocationOfficePageContent>>> Put(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, [FromBody] LocationOfficePageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _locationOfficeCrudService.UpdatePageAsync(channelId, PageGuid, culture, request, cancellationToken);

            return Ok(pageDto.Value);
        }

        /// <summary>
        /// Deletes a single location office page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be deleted.</param>
        /// <param name="PageGuid">Page GUID of the location office.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="deleteAllCultures">Should all culture versions be deleted of this location office?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>204 No Content.</returns>
        [HttpDelete("{PageGuid:guid}")]
        [HasPermission(ApiMethodGroupName.LocationOffices, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
        public async Task<IActionResult> Delete(int memberFirmID, Guid PageGuid, [FromQuery][Required] string culture, [FromQuery] bool deleteAllCultures = false, CancellationToken cancellationToken = default)
        {
            try
            {
                int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
                await _locationOfficeCrudService.DeletePageAsync(channelId, PageGuid, culture, deleteAllCultures, cancellationToken);

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