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
    /// Provides methods to read and manage deals.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/Deals")]
    public class DealsController : EntityControllerBase
    {
        private readonly IDealCrudService _dealCrudService;

        public DealsController(IDealCrudService dealCrudService)
        {
            _dealCrudService = dealCrudService;
        }

        /// <summary>
        /// Returns all deal pages.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the pages should be retrieved.</param>
        /// <param name="culture">If specified, pages will be retrieved only of this culture. Otherwise pages of all cultures will be retrieved.</param>
        /// <param name="onlyPublished">If false, both published and unpublished pages will be retrieved. Otherwise only published pages will be retrieved.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a list of deals.</returns>
        [HttpGet]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageOutput<DealPageContent>>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> Get(int memberFirmID, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pages = await _dealCrudService.GetDealPagesAsync(channelId, culture, onlyPublished, cancellationToken);

            return Ok(pages);
        }

        /// <summary>
        /// Returns a single deal having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
        /// <param name="pageGUID">Page GUID of an deal page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns one deal page.</returns>
        [HttpGet("{pageGUID:guid}")]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<DealPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<DealPageContent>>> Get(int memberFirmID, Guid pageGUID, [FromQuery][Required] string culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _dealCrudService.GetDealPageAsync(channelId, pageGUID, culture, cancellationToken);

            return Ok(page);
        }

        /// <summary>
        /// Creates a new deal page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="nodeParentID">Node ID of the page that will contain the created deal page.
        /// <br/><br/>This value can be taken from <b>Kentico Pages module</b> by selecting the parent. Select <b>General tab</b>, then click <b>Advanced</b>. The <b>Node ID</b> property will contain the required value.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Input data of the new page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Newly created deal page.</returns>
        [HttpPost]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<DealPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<DealPageContent>>> Post(int memberFirmID, [FromQuery][Required] int nodeParentID, [FromQuery][Required] string culture, [FromBody] DealPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _dealCrudService.CreatePageAsync(channelId, nodeParentID, culture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Creates a new culture version of a deal page.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
        /// <param name="pageGUID">Page GUID of the deal to copy content from.</param>
        /// <param name="fromCulture">Culture of the existing page version.</param>
        /// <param name="toCulture">Culture of the new page version.</param>
        /// <param name="request">New culture version data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>New deal page in another culture.</returns>
        [HttpPost("{pageGUID:guid}/CreateNewCultureVersion")]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<DealPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<DealPageContent>>> CreateNewCultureVersion(int memberFirmID, Guid pageGUID, [FromQuery][Required] string fromCulture, [FromQuery][Required] string toCulture, [FromBody] DealPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _dealCrudService.CreatePageCultureVersionAsync(channelId, pageGUID, fromCulture, toCulture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.PageGuid }, pageDto.Value);
        }

        /// <summary>
        /// Updates a single deal page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be modified.</param>
        /// <param name="pageGUID">Page GUID of the deal page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="request">Updated deal page data.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated deal page.</returns>
        [HttpPut("{pageGUID:guid}")]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<DealPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<DealPageContent>>> Put(int memberFirmID, Guid pageGUID, [FromQuery][Required] string culture, [FromBody] DealPageContent request, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _dealCrudService.UpdatePageAsync(channelId, pageGUID, culture, request, cancellationToken);

            return Ok(pageDto.Value);
        }

        /// <summary>
        /// Deletes a single deal page having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm in which the page should be deleted.</param>
        /// <param name="pageGUID">Page GUID of the deal.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="deleteAllCultures">Should all culture versions be deleted of this deal?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>204 No Content.</returns>
        [HttpDelete("{pageGUID:guid}")]
        [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Manage)]
        [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
        public async Task<IActionResult> Delete(int memberFirmID, Guid pageGUID, [FromQuery][Required] string culture, [FromQuery] bool deleteAllCultures = false, CancellationToken cancellationToken = default)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            await _dealCrudService.DeletePageAsync(channelId, pageGUID, culture, deleteAllCultures, cancellationToken);

            return NoContent();
        }
    }
}
