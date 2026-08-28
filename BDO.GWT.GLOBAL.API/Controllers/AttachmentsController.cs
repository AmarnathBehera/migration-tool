
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    [Route("/memberFirms/{memberFirmID:int}/Pages/{pageGUID:guid}/[controller]")]
    public class AttachmentsController : EntityControllerBase
    {
        private readonly IPageAttachmentCrudService attachmentService;

        public AttachmentsController(
            IPageAttachmentCrudService attachmentService)
        {
            this.attachmentService = attachmentService;
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
       [FromForm] CreateAttachmentRequest request,
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
                return NotFound();
            }

            Response.GetTypedHeaders().ContentDisposition = new ContentDispositionHeaderValue("attachment");

            return File(result.Value.Data, MediaTypeNames.Application.Octet);
        }
        public class CreateAttachmentRequest
        {
            [Required]
            public string Culture { get; set; }

            [Required]
            public IFormFile File { get; set; }
        }
    }
}
