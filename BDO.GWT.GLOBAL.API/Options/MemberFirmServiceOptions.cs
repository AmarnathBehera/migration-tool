using System;

namespace BDO.GWT.GLOBAL.API.Options;

public class MemberFirmServiceOptions
{
    public TimeSpan CachingPeriod { get; set; }

    public bool IncludeMemberFirmsJson { get; set; }
}
