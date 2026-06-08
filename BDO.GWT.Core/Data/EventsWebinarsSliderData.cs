using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Data;

public class EventsWebinarsSliderData 
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool FilteringEnabled { get; set; }

    public HashSet<string> Filters { get; set; } = new();

    public IEnumerable<Guid> FeaturedEventAndWebinarNodeGuids { get; set; } = Enumerable.Empty<Guid>();

    public Guid? BackgroundImageMediaFileGuid { get; set; }

    public string? BackgroundImageAltText { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
    public string? WidgetID { get; set; }

}
