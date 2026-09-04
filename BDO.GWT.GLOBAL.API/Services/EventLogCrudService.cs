using BDO.GWT.Domain.Core.Repositories;
using CMS.DataEngine;
using CMS.EventLog;
using System.Diagnostics;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IEventLogCrudService
{
    Task DeleteAllEventLogEntriesAsync(CancellationToken cancellationToken);
}

public class EventLogCrudService : IEventLogCrudService
{
    private readonly IInfoProvider<EventLogInfo> _eventLogInfoProvider;

    public EventLogCrudService(IInfoProvider<EventLogInfo> eventLogInfoProvider)
    {
        _eventLogInfoProvider = eventLogInfoProvider;
    }

    public Task DeleteAllEventLogEntriesAsync(CancellationToken cancellationToken)
    {
        _eventLogInfoProvider.BulkDelete(null);

        return Task.CompletedTask;
    }
}