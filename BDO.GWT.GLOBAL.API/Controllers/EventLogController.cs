using BDO.GWT.Domain.Core.Enums;
using BDO.GWT.GLOBAL.API.Attributes;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BDO.GWT.GLOBAL.API.Controllers;

/// <summary>
/// Provides methods to read and manage event log.
/// </summary>
[AuthorizeUserPrivilegeLevel(UserPrivilegeLevel.GlobalAdministrator)]
[Route("EventLog")]
public class EventLogController : EntityControllerBase
{
    private readonly IEventLogCrudService _eventLogCrudService;

    public EventLogController(IEventLogCrudService eventLogCrudService)
    {
        _eventLogCrudService = eventLogCrudService;
    }

    /// <summary>
    /// Deletes all event log entries.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>204 No Content.</returns>
    [HttpDelete]
    [SwaggerResponse(StatusCodes.Status204NoContent, null, typeof(void))]
    public async Task<ActionResult> DeleteAllEventLogEntries(CancellationToken cancellationToken)
    {
        await _eventLogCrudService.DeleteAllEventLogEntriesAsync(cancellationToken);

        return NoContent();
    }
}