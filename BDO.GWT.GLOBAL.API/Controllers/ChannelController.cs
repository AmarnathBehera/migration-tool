using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.Web.Core.API.Controllers;

/// <summary>
/// Provides methods to read and manage channels.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("memberFirms/{memberFirmID:int}/Channels")]
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
    /// <param name = "cancellationToken" ></ param >
    /// < returns > Returns a list of channels.</returns>
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
    /// <param name="channelGUID">GUID of the channel you want to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one channel.</returns>
    [HttpGet("{channelGUID:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(ChannelOutput), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ChannelOutput>> GetChannelAsync(Guid channelGUID, CancellationToken cancellationToken)
    {
        var channel = await _channelCrudService.GetEntityAsync(channelGUID, cancellationToken);

        return Ok(channel);
    }
}
