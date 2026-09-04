using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Attributes;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers;

/// <summary>
/// Provides methods to read and manage scheduled tasks.
/// </summary>
[ApiController]
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
[Route("ScheduledTasks")]
public class ScheduledTasksController : EntityControllerBase
{
    private readonly IScheduledTaskCrudService _scheduledTaskCrudService;

    public ScheduledTasksController(IScheduledTaskCrudService scheduledTaskCrudService)
    {
        _scheduledTaskCrudService = scheduledTaskCrudService;
    }

    /// <summary>
    /// Returns all scheduled tasks.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a list of scheduled tasks.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(IEnumerable<ScheduledTaskDto>), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<ScheduledTaskDto>>> GetScheduledTasksAsync(CancellationToken cancellationToken)
    {
        var scheduledTasks = await _scheduledTaskCrudService.GetScheduledTasksDtosAsync(cancellationToken);

        return Ok(scheduledTasks);
    }

    /// <summary>
    /// Returns a single scheduled task having the specified GUID.
    /// </summary>
    /// <param name="scheduledTaskGuid">GUID of the scheduled task you want to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns one scheduled task.</returns>
    [HttpGet("{scheduledTaskGuid:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns one scheduled task.", typeof(IEnumerable<ScheduledTaskDto>), MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Returns when scheduled task with the specified GUID is not found.", typeof(ProblemDetails), MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ScheduledTaskDto>> GetScheduledTaskAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken)
    {
        var scheduledTask = await _scheduledTaskCrudService.GetScheduledTaskDtoAsync(scheduledTaskGuid, cancellationToken);

        return Ok(scheduledTask);
    }

    }
