
using CMS.ContentEngine;

namespace BDO.GWT.Core.Data;

public class EditorsPickData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool FilteringEnabled { get; set; }

    public HashSet<string> Filters { get; set; } = new();

    public HashSet<string> SelectedPageTypes { get; set; } = new();

    public IEnumerable<ContentItemReference> EditorsPickNodeGuids { get; set; } = Enumerable.Empty<ContentItemReference>();

    public IEnumerable<ContentItemReference> FeaturedPageNodeGuids { get; set; } = Enumerable.Empty<ContentItemReference>();

    public Guid? BackgroundImageMediaFileGuid { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
