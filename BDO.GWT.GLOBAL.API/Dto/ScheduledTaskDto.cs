namespace BDO.GWT.GLOBAL.API.DTOs;

public class ScheduledTaskDto
{
    public required int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Interval { get; set; }
    public string? Data { get; set; }

    /// <summary>Identifies the IScheduledTask implementation (registered via RegisterScheduledTask). Replaces the old AssemblyName + Class pair.</summary>
    public required string ScheduledTaskIdentifier { get; set; }

    public DateTime? LastRunTime { get; set; }
    public DateTime? NextRunTime { get; set; }
    public string? LastResult { get; set; }
    public bool DeleteAfterLastRun { get; set; }

    /// <summary>Server currently or last executing this task.</summary>
    public string? ExecutingServerName { get; set; }

    public required DateTime LastModified { get; set; }
    public int ExecutionCount { get; set; }
    public int? UserId { get; set; }
    public string? ObjectType { get; set; }
    public int? ObjectId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsRunning { get; set; }
    public bool IsCancellationRequested { get; set; }

    /// <summary>True if the task configuration row was generated dynamically at runtime rather than authored via the admin UI.</summary>
    public bool CreatedDynamically { get; set; }
}