
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    [Route("/memberFirms/{memberFirmID:int}/Pages/{pageGUID:guid}/[controller]")]
    public class AttachmentsController : EntityControllerBase
    {
        private readonly IPageAttachmentCrudService attachmentService;
        private readonly ILogger<AttachmentsController> logger;

        public AttachmentsController(
            IPageAttachmentCrudService attachmentService,
            ILogger<AttachmentsController> logger)
        {
            this.attachmentService = attachmentService;
            this.logger = logger;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Gets all attachments for a page")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, "List of attachments", typeof(IEnumerable<PageAttachmentOutput>), MediaTypeNames.Application.Json)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Get(
            int memberFirmID,
            Guid pageGUID,
            [FromQuery][Required] string culture,
            CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var result =
                await attachmentService.GetAttachmentsAsync(
                    channelId,
                    pageGUID,
                    culture,
                    cancellationToken);

            return Ok(result);
        }

        [HttpGet("{attachmentGUID:guid}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Gets a specific attachment by GUID")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, "Attachment details", typeof(PageAttachmentOutput), MediaTypeNames.Application.Json)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Attachment or page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Get(
            int memberFirmID,
            Guid pageGUID,
            Guid attachmentGUID,
            [FromQuery][Required] string culture,
            CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var result =
                await attachmentService.GetAttachmentAsync(
                    channelId,
                    pageGUID,
                    attachmentGUID,
                    culture,
                    cancellationToken);

            return result == null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Creates a new attachment for a page")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status201Created, "Attachment created", typeof(PageAttachmentOutput), MediaTypeNames.Application.Json)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Post(
       [FromRoute] int memberFirmID,
       [FromRoute] Guid pageGUID,
       [FromForm] AttachmentRequest request,
       CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var result = await attachmentService.CreateAttachmentAsync(
                channelId,
                pageGUID,
                request.Culture,
                request.File,
                cancellationToken);

            return CreatedAtAction(nameof(Get), new { memberFirmID, pageGUID, attachmentGUID = result.GUID }, result);
        }

        [HttpGet("{attachmentGUID:guid}/binary")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Gets attachment binary content")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, "Attachment file content", typeof(byte[]), MediaTypeNames.Application.Octet)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Attachment or page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> GetBinary(
            int memberFirmID,
            Guid pageGUID,
            Guid attachmentGUID,
            [FromQuery][Required] string culture,
            CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var result = await attachmentService.GetAttachmentBinaryAsync(
                channelId,
                pageGUID,
                attachmentGUID,
                culture,
                cancellationToken);

            if (result == null)
            {
                logger.LogWarning(
                    "Attachment binary not found or unavailable. MemberFirmID: {MemberFirmID}, PageGUID: {PageGUID}, AttachmentGUID: {AttachmentGUID}, Culture: {Culture}, ChannelId: {ChannelId}",
                    memberFirmID,
                    pageGUID,
                    attachmentGUID,
                    culture,
                    channelId);
                return NotFound();
            }

            Response.GetTypedHeaders().ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = result.Value.FileName
            };

            return File(result.Value.Data, result.Value.ContentType, result.Value.FileName);
        }

        [HttpPut("{attachmentGUID:guid}")]
        [Consumes("multipart/form-data")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Updates an existing attachment for a page")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, "Attachment updated", typeof(PageAttachmentOutput), MediaTypeNames.Application.Json)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Attachment or page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Put(
            [FromRoute] int memberFirmID,
            [FromRoute] Guid pageGUID,
            [FromRoute] Guid attachmentGUID,
            [FromForm] AttachmentRequest request,
            CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var result = await attachmentService.UpdateAttachmentAsync(
                channelId,
                pageGUID,
                attachmentGUID,
                request.Culture,
                request.File,
                cancellationToken);

            return result == null
                ? NotFound()
                : Ok(result);
        }


        [HttpDelete("{attachmentGUID:guid}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(Summary = "Deletes an attachment from a page")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status204NoContent, "Attachment deleted")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status404NotFound, "Attachment or page not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
        public async Task<IActionResult> Delete(
            [FromRoute] int memberFirmID,
            [FromRoute] Guid pageGUID,
            [FromRoute] Guid attachmentGUID,
            [FromQuery][Required] string culture,
            CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);

            var deleted = await attachmentService.DeleteAttachmentAsync(
                channelId,
                pageGUID,
                attachmentGUID,
                culture,
                cancellationToken);

            return deleted ? NoContent() : NotFound();
        }
    }
}
