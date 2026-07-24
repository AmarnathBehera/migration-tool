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

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to read and manage event sections.
/// </summary>
[AllowAnonymous]
[Route("memberFirms/{memberFirmID:int}/EventSections")]
public class EventSectionsController : EntityControllerBase
{
    private readonly ISectionEventsCrudService _sectionEventsCrudService;

    public EventSectionsController(ISectionEventsCrudService sectionEventsCrudService)
    {
        _sectionEventsCrudService = sectionEventsCrudService;
    }

    /// <summary>
    /// Returns all section events pages.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which the pages should be retrieved.</param>
    /// <param name="culture">If specified, pages will be retrieved only of this culture. Otherwise pages of all cultures will be retrieved.</param>
    /// <param name="onlyPublished">If false, both published and unpublished pages will be retrieved. Otherwise only published pages will be retrieved.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a list of event sections.</returns>
    [HttpGet]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageOutput<SectionEventsPageContent>>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<PageOutput<SectionEventsPageContent>>>> Get(int memberFirmID, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pages = await _sectionEventsCrudService.GetPagesAsync(channelId, culture, onlyPublished, cancellationToken);

            return Ok(pages);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Returns a single section events having the specified page GUID.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
    /// <param name="WebPageItemGuid">WebPageItem GUID of an section events page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one section events page.</returns>
    [HttpGet("{WebPageItemGuid:guid}")]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Read)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<SectionEventsPageContent>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> Get(int memberFirmID, Guid WebPageItemGuid, [FromQuery][Required] string culture, CancellationToken cancellationToken)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _sectionEventsCrudService.GetPageAsync(channelId, WebPageItemGuid, culture, cancellationToken);

            return Ok(page);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Creates a new section events page.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
    /// <param name="WebPageParentID">WebPageParent ID of the page that will contain the created section events page.
    /// <br/><br/>This value can be taken from <b>Kentico Pages module</b> by selecting the parent. Select <b>General tab</b>, then click <b>Advanced</b>. The <b>Node ID</b> property will contain the required value.</param>
    /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
    /// <param name="request">Input data of the new page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Newly created section events page.</returns>
    [HttpPost]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<SectionEventsPageContent>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> Post(int memberFirmID, [FromQuery][Required] int WebPageParentID, [FromQuery][Required] string culture, [FromBody] SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _sectionEventsCrudService.CreatePageAsync(channelId,WebPageParentID, culture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.WebPageItemGuid }, pageDto);
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

    /// <summary>
    /// Creates a new culture version of a section events page.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm in which the page should be created.</param>
    /// <param name="WebPageItemGuid">WebPageItem GUID of the section events to copy content from.</param>
    /// <param name="fromCulture">Culture of the old page version.</param>
    /// <param name="toCulture">Culture of the new page version.</param>
    /// <param name="request">New culture version data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>New section events page in another culture.</returns>
    [HttpPost("{WebPageItemGuid:guid}/CreateNewCultureVersion")]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(PageOutput<SectionEventsPageContent>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> CreateNewCultureVersion(int memberFirmID, Guid WebPageItemGuid, [FromQuery][Required] string fromCulture, [FromQuery][Required] string toCulture, [FromBody] SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _sectionEventsCrudService.CreatePageCultureVersionAsync(channelId, WebPageItemGuid, fromCulture, toCulture, request, cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageDto.Value?.General?.WebPageItemGuid }, pageDto);
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

    /// <summary>
    /// Updates a single section events page having the specified page GUID.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm in which the page should be modified.</param>
    /// <param name="WebPageItemGuid">WebPageItem GUID of the section events page.</param>
    /// <param name="request">Updated section events page data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Updated section events page.</returns>
    [HttpPut("{WebPageItemGuid:guid}")]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<SectionEventsPageContent>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<PageOutput<SectionEventsPageContent>>> Put(int memberFirmID, Guid WebPageItemGuid, [FromQuery][Required] string culture, [FromBody] SectionEventsPageContent request, CancellationToken cancellationToken)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var pageDto = await _sectionEventsCrudService.UpdatePageAsync(channelId, WebPageItemGuid, culture, request, cancellationToken);

            return Ok(pageDto);
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

    /// <summary>
    /// Deletes a single section events page having the specified page GUID.
    /// </summary>
    /// <param name="memberFirmID">ID of the member firm in which the page should be deleted.</param>
    /// <param name="WebPageItemGuid">WebPageItem GUID of the section events.</param>
    /// <param name="deleteAllCultures">Should all culture versions be deleted of this section events?</param>
    /// <param name="cancellationToken"></param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{WebPageItemGuid:guid}")]
    [HasPermission(ApiMethodGroupName.EventSections, ApiPermission.Manage)]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> Delete(int memberFirmID, Guid WebPageItemGuid, [FromQuery][Required] string culture, [FromQuery] bool deleteAllCultures = false, CancellationToken cancellationToken = default)
    {
        try
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            await _sectionEventsCrudService.DeletePageAsync(channelId, WebPageItemGuid, culture, deleteAllCultures, cancellationToken);

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