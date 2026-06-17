

namespace BDO.GWT.Core.Data.Rss;

public class RssChannel
{
    public string Title { get; set; } = null!;

    public string Link { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Language { get; set; }

    public string? Category { get; set; }

    public ICollection<RssChannelItem> Items { get; set; } = new List<RssChannelItem>();
}
