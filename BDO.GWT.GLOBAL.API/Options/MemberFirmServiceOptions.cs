namespace BDO.GWT.GLOBAL.API.Options;

/// <summary>
/// Represents the configuration options for the Member Firm Service, including caching period and whether to include member firms in JSON format.
/// </summary>
public class MemberFirmServiceOptions
{
    public TimeSpan CachingPeriod { get; set; }

    public bool IncludeMemberFirmsJson { get; set; }
}
