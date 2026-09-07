using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

[ApiController]
[Route("Channels")]
public class ChannelController : EntityControllerBase
{
    private readonly IChannelCrudService _channelCrudService;

    public ChannelController(IChannelCrudService channelCrudService)
    {
        _channelCrudService = channelCrudService;
    }

    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<ChannelOutput>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<ChannelOutput>>> GetChannelsAsync([FromQuery] ChannelsQuery query, CancellationToken cancellationToken)
    {
        var channels = await _channelCrudService.GetEntitiesAsync(query, cancellationToken);

        return Ok(channels);
    }


    [HttpGet("{channelGuid:guid}", Name = "GetChannel")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> GetChannelAsync(Guid channelGuid, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.GetEntityAsync(channelGuid, cancellationToken);

        return Ok(channel);
    }


    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> PostChannelAsync(
        [FromBody] ChannelInput request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.CreateEntityAsync(request, cancellationToken);

        if (channel.General.GUID == Guid.Empty)
        {
            return BadRequest("Channel GUID was not generated.");
        }

        return CreatedAtRoute(
            "GetChannel",
            new { channelGuid = channel.General.GUID },
            channel);
    }

    [HttpPut("{channelGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> PutChannelAsync(Guid channelGuid, [FromBody] ChannelInput request, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.UpdateEntityAsync(channelGuid, request, cancellationToken);

        return Ok(channel);
    }


    [HttpDelete("{channelGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<IActionResult> DeleteChannelAsync(Guid channelGuid, CancellationToken cancellationToken)
    {
        await _channelCrudService.DeleteEntityAsync(channelGuid, cancellationToken);

        return NoContent();
    }
}
