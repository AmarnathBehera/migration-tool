
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Net.Http.Headers;
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<PageAttachmentOutput>), MediaTypeNames.Application.Json)]
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageAttachmentOutput), MediaTypeNames.Application.Json)]
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK, null, typeof(byte[]), MediaTypeNames.Application.Octet)]
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

            return File(result.Value.Data, MediaTypeNames.Application.Octet, result.Value.FileName);
        }
    }
}
