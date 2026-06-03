

namespace BDO.GWT.Xperience.Core.Services;

public interface ITimeZoneService
{
    DateTime ConvertToSiteDateTime(DateTime dateTime);
}

public class TimeZoneService : ITimeZoneService
{

    public DateTime ConvertToSiteDateTime(DateTime dateTime)
    {
        var utc = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
    }
}
