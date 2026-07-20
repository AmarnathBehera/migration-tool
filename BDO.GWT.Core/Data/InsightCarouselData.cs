using CMS.ContentEngine;

namespace BDO.GWT.Web.Core.Data;

public class InsightCarouselData 
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool FilteringEnabled { get; set; }

    public HashSet<string> Filters { get; set; } = new();
    public IEnumerable<ContentItemReference> FeaturedInsightNodeGuids { get; set; } = Enumerable.Empty<ContentItemReference>();

    public string? BackgroundImageURL { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
    public string? WidgetID { get; set; }

}
