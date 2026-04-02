using System;

namespace BDO.GWT.Common.Options;

public class CachingOptions
{
    public bool CachingEnabled { get; set; } = false;

    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(60);

    public ReactCachingOptions React { get; set; } = new();
}
