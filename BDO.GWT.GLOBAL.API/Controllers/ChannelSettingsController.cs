using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Returns all channel settings.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm for which the channel settings should be retrieved.</param>
        /// <param name="query">Optional query parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns a list of channel settings.</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelSettingOutput[]), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<ChannelSettingOutput[]>> GetChannelSettings(int memberFirmID, [FromQuery] SettingsQuery query, CancellationToken cancellationToken)
        {
            var channelSettings = await _channelSettingCrudService.GetChannelSettings(query, cancellationToken);

            return Ok(channelSettings);
        }

        /// <summary>
        /// Returns a single channel setting having the specified code name.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm for which the channel setting should be retrieved.</param>
        /// <param name="codeName">Code name of the channel setting you want to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns one channel setting.</returns>
        [HttpGet("{codeName}")]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelSettingOutput), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<ChannelSettingOutput[]>> GetChannelSetting(int memberFirmID, string codeName, CancellationToken cancellationToken)
        {
            var channelSetting = await _channelSettingCrudService.GetChannelSetting(codeName, cancellationToken);

            return Ok(channelSetting);
        }
    }
}