using CMS.Base;
using CMS.Core;


namespace BDO.GWT.Core.Services;

public class EventLogging : IEventLogging
{
    private readonly IEventLogService _eventLogService;

    public EventLogging(IEventLogService eventLogService)
    {
        _eventLogService = eventLogService;
    }

    public void LogEvent(EventLogData data)
    {
        var exceptionDescription = data.Exception is not null
            ? $"Message: {data.Exception.Message}\n\nException type: {data.Exception.GetType().FullName}\nStack trace:\n{data.Exception.StackTrace}"
            : null;

        string?[] eventDescriptionParagraphs =
            [data.EventDescription, exceptionDescription];

        var eventLogInfo = new EventLogData(
            data.EventType,
            data.Source,
            data.EventCode
        )
        {
            EventTime = DateTime.Now,
            Exception = data.Exception,
            EventDescription = string.Join("\n\n", eventDescriptionParagraphs.OfType<string>()),
            EventMachineName = SystemContext.MachineName,
        };

        _eventLogService.LogEvent(eventLogInfo);
    }
}