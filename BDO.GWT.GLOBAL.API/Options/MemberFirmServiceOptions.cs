namespace BDO.GWT.GLOBAL.API.Options;

/// <summary>
/// Represents the configuration options for the Member Firm Service, including caching period and whether to include member firms in JSON format.
/// </summary>
public class MemberFirmServiceOptions
{
    /// <summary>
    /// Gets or sets the caching period for member firm data.
    /// </summary>
    public TimeSpan CachingPeriod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include member firms from the JSON configuration file.
    /// </summary>
    public bool IncludeMemberFirmsJson { get; set; }
}
