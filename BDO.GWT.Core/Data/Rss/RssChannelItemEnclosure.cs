using System;

namespace BDO.GWT.Core.Data.Rss;

public class RssChannelItemEnclosure
{
    public long Length { get; set; }

    public string Type { get; set; } = null!;

    public Uri Url { get; set; } = null!;
}
