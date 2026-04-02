using System;

namespace BDO.GWT.Common.Options;

public class ReactCachingOptions
{
    public bool CachingEnabled { get; set; } = false;

    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(60);
}