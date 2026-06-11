namespace BDO.GWT.Core.LiveSite.Data;

public class NewsCarouselData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool FilteringEnabled { get; set; }

    public HashSet<string> Filters { get; set; } = [];

    public IEnumerable<Guid> NodeGuidsOfFeaturedNews { get; set; } = Enumerable.Empty<Guid>();

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
