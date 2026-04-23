using CMS.Core;

namespace BDO.GWT.Core.Services;

public interface IEventLogging
{
    public void LogEvent(EventLogData data);
}
