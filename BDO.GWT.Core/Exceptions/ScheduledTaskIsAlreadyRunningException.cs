namespace BDO.GWT.Core.Exceptions;

public class ScheduledTaskIsAlreadyRunningException(string? description = null) : Exception(description ?? "Scheduled task is already running.");
