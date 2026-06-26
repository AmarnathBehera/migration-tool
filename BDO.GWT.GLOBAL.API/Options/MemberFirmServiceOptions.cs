using System;

namespace BDO.GWT.Web.Core.API.Options;

public class MemberFirmServiceOptions
{
    public TimeSpan CachingPeriod { get; set; }

    public bool IncludeMemberFirmsJson { get; set; }
}
