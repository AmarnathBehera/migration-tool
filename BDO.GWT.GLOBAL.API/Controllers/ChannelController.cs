using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to read and manage channels.
/// </summary>
[ApiController]
[Route("Channels")]
public class ChannelController : EntityControllerBase
{
    private readonly IChannelCrudService _channelCrudService;

    public ChannelController(IChannelCrudService channelCrudService)
    {
        _channelCrudService = channelCrudService;
    }

    /// <summary>
    /// Returns all channels.
    /// </summary>
    /// <param name = "query" > Query parameters.</param>
    /// <param name = "cancellationToken" ></param >
    /// <returns > Returns a list of channels.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<ChannelOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<ChannelOutput>>> GetChannelsAsync([FromQuery] ChannelsQuery query, CancellationToken cancellationToken)
    {
        var channels = await _channelCrudService.GetEntitiesAsync(query, cancellationToken);

        return Ok(channels);
    }

    /// <summary>
    /// Returns a single channel having the specified GUID.
    /// </summary>
    /// <param name="channelGuid">GUID of the channel you want to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one channel.</returns>
    [HttpGet("{channelGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> GetChannelAsync(Guid channelGuid, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.GetEntityAsync(channelGuid, cancellationToken);

        return Ok(channel);
    }

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="request">Input data of the new channel.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Newly created channel.</returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> PostChannelAsync([FromRoute] int memberFirmID, [FromBody] ChannelInput request, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.CreateEntityAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetChannelAsync), new { memberFirmID, channelGuid = channel.General.GUID }, channel);
    }

    //            return CreatedAtAction(nameof(Get), new { memberFirmID, pageGUID, attachmentGUID = result.GUID }, result);


    /// <summary>
    /// Updates a single channel having the specified GUID.
    /// </summary>
    /// <param name="channelGuid">GUID of the channel.</param>
    /// <param name="request">Updated channel data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Updated channel.</returns>
    [HttpPut("{channelGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> PutChannelAsync(Guid channelGuid, [FromBody] ChannelInput request, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.UpdateEntityAsync(channelGuid, request, cancellationToken);

        return Ok(channel);
    }

    /// <summary>
    /// Deletes a single channel having the specified GUID.
    /// </summary>
    /// <param name="channelGuid">GUID of the channel.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{channelGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> DeleteChannelAsync(Guid channelGuid, CancellationToken cancellationToken)
    {
        await _channelCrudService.DeleteEntityAsync(channelGuid, cancellationToken);

        return NoContent();
    }
}
