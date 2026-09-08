using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using ChannelSettingInput = BDO.GWT.Web.Core.API.Data.ChannelSettingInput;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/ChannelSettings")]
    public class ChannelSettingsController : EntityControllerBase
    {
        private readonly IChannelSettingCrudService _channelSettingCrudService;

        public ChannelSettingsController(IChannelSettingCrudService channelSettingCrudService)
        {
            _channelSettingCrudService = channelSettingCrudService;
        }


        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelSettingOutput[]), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<ChannelSettingOutput[]>> GetChannelSettings(int memberFirmID, [FromQuery] SettingsQuery query, CancellationToken cancellationToken)
        {
            var webChannelSnippetWebChannelSettingsId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var channelSettings = await _channelSettingCrudService.GetChannelSettings(webChannelSnippetWebChannelSettingsId, query, cancellationToken);

            return Ok(channelSettings);
        }


        [HttpGet("{codeName}")]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelSettingOutput), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<ChannelSettingOutput[]>> GetChannelSetting(int memberFirmID, string codeName, CancellationToken cancellationToken)
        {
            var webChannelSnippetWebChannelSettingsId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var channelSetting = await _channelSettingCrudService.GetChannelSetting(webChannelSnippetWebChannelSettingsId, codeName, cancellationToken);

            return Ok(channelSetting);
        }

        [HttpPut("{codeName}")]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelSettingOutput), MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(void))]
        public async Task<ActionResult<ChannelSettingOutput>> PutChannelSetting(
            int memberFirmID,
            string codeName,
            [FromBody] ChannelSettingInput request,
            CancellationToken cancellationToken)
        {
            var webChannelSnippetWebChannelSettingsId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var updatedSetting = await _channelSettingCrudService.UpdateChannelSetting(webChannelSnippetWebChannelSettingsId, codeName, request, cancellationToken);

            return updatedSetting is null
                ? NotFound()
                : Ok(updatedSetting);
        }
    }
}