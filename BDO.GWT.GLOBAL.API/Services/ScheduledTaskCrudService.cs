using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core.Exceptions;
using BDO.GWT.GLOBAL.API.DTOs;
using CMS.DataEngine;
using CMS.Scheduler;
using CMS.Scheduler.Internal;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IScheduledTaskCrudService
{
    Task<ScheduledTaskDto[]> GetScheduledTasksDtosAsync(CancellationToken cancellationToken);

    Task<ScheduledTaskDto> GetScheduledTaskDtoAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken);

    Task ExecuteScheduledTaskAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken);

}

public class ScheduledTaskCrudService : IScheduledTaskCrudService
{
    private readonly IInfoProvider<ScheduledTaskConfigurationInfo> _scheduledTaskConfigurationInfoProvider;
    private readonly IValidationState _validationState;
    private readonly ISchedulingExecutor _schedulingExecutor;

    public ScheduledTaskCrudService(
        IInfoProvider<ScheduledTaskConfigurationInfo> scheduledTaskConfigurationInfoProvider,
        IValidationState validationState,
        ISchedulingExecutor schedulingExecutor)
    {
        _scheduledTaskConfigurationInfoProvider = scheduledTaskConfigurationInfoProvider;
        _validationState = validationState;
        _schedulingExecutor = schedulingExecutor;
    }

    public async Task<ScheduledTaskDto[]> GetScheduledTasksDtosAsync(CancellationToken cancellationToken)
    {
        var scheduledTasks = await _scheduledTaskConfigurationInfoProvider.Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return scheduledTasks.Select(ToScheduledTaskDto).ToArray();
    }

    public async Task<ScheduledTaskDto> GetScheduledTaskDtoAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken)
    {
        var scheduledTask = await GetValidatedScheduledTaskAsync(scheduledTaskGuid, cancellationToken);

        return ToScheduledTaskDto(scheduledTask);
    }

    private async Task<ScheduledTaskConfigurationInfo> GetValidatedScheduledTaskAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken)
    {
        var scheduledTask = (await _scheduledTaskConfigurationInfoProvider.Get()
            .WhereEquals(nameof(ScheduledTaskConfigurationInfo.ScheduledTaskConfigurationGUID), scheduledTaskGuid)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (scheduledTask is null)
        {
            _validationState.AddError("scheduledTaskGuid", $"Scheduled task with '{scheduledTaskGuid}' GUID not found.");

            throw new NotFoundException();
        }

        return scheduledTask;
    }
    public async Task ExecuteScheduledTaskAsync(Guid scheduledTaskGuid, CancellationToken cancellationToken)
    {
        var scheduledTask = await GetValidatedScheduledTaskAsync(scheduledTaskGuid, cancellationToken);

        try
        {
            await _schedulingExecutor.ExecuteTask(scheduledTask.ScheduledTaskConfigurationID, cancellationToken);
        }
        catch (ScheduledTaskIsRunningException)
        {
            _validationState.AddError("scheduledTaskGuid", $"Scheduled task with '{scheduledTaskGuid}' GUID is already running.");

            throw new ScheduledTaskIsAlreadyRunningException();
        }
    }
    private static ScheduledTaskDto ToScheduledTaskDto(ScheduledTaskConfigurationInfo scheduledTask)
    {
        return new ScheduledTaskDto
        {
            Id = scheduledTask.ScheduledTaskConfigurationID,
            Guid = scheduledTask.ScheduledTaskConfigurationGUID,
            Name = scheduledTask.ScheduledTaskConfigurationName,
            DisplayName = scheduledTask.ScheduledTaskConfigurationDisplayName,
            Interval = scheduledTask.ScheduledTaskConfigurationInterval,
            Data = scheduledTask.ScheduledTaskConfigurationData,
            ScheduledTaskIdentifier = scheduledTask.ScheduledTaskConfigurationScheduledTaskIdentifier,
            LastRunTime = scheduledTask.ScheduledTaskConfigurationLastRunTime,
            NextRunTime = scheduledTask.ScheduledTaskConfigurationNextRunTime,
            LastResult = scheduledTask.ScheduledTaskConfigurationLastResult,
            DeleteAfterLastRun = scheduledTask.ScheduledTaskConfigurationDeleteAfterLastRun,
            ExecutingServerName = scheduledTask.ScheduledTaskConfigurationExecutingServerName,
            LastModified = scheduledTask.ScheduledTaskConfigurationLastModified,
            ExecutionCount = scheduledTask.ScheduledTaskConfigurationExecutions,
            UserId = scheduledTask.ScheduledTaskConfigurationUserID,
            ObjectType = scheduledTask.ScheduledTaskConfigurationObjectType,
            ObjectId = scheduledTask.ScheduledTaskConfigurationObjectID,
            IsEnabled = scheduledTask.ScheduledTaskConfigurationEnabled,
            IsRunning = scheduledTask.ScheduledTaskConfigurationIsRunning,
            IsCancellationRequested = scheduledTask.ScheduledTaskConfigurationIsCancellationRequested,
            CreatedDynamically = scheduledTask.ScheduledTaskConfigurationCreatedDynamically,
        };
    }
}