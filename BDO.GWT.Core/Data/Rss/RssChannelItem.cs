using System;
using System.Collections.Generic;

namespace BDO.GWT.Core.Data.Rss;

public class RssChannelItem
{
    public string? Author { get; set; }

    public List<string> Categories { get; set; } = new();

    public string? Description { get; set; }

    public RssChannelItemEnclosure? Enclosure { get; set; }

    public string? GUID { get; set; }

    public Uri? Link { get; set; }

    public DateTimeOffset? PubDate { get; set; }

    public string? Title { get; set; }
}
