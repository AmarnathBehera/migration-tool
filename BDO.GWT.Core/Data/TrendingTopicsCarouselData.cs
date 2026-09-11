using CMS.ContentEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Data;

public class TrendingTopicsCarouselData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<ContentItemReference> TrendingTopicsPages { get; set; } = Enumerable.Empty<ContentItemReference>();

    public string? LearnMoreButtonURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
