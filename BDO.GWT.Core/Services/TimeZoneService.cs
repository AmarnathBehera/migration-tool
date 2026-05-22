

namespace BDO.GWT.Xperience.Core.Services;

public interface ITimeZoneService
{
    DateTime ConvertToSiteDateTime(DateTime dateTime);
}

/// <summary>
/// Converts a UTC <see cref="DateTime"/> value to the site's local time.
/// </summary>
/// <remarks>
/// In Xperience by Kentico the legacy per-site <c>SiteInfo</c>/<c>TimeZoneHelper</c>
/// APIs have been removed and there is no per-channel time zone configuration,
/// so the server's local time zone is used as the "site" time zone.
/// </remarks>
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
