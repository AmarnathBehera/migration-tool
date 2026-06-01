using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.Data;

public class FlexibleContentData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<Guid> FlexibleContentGuids { get; set; } = [];

    public string? LearnMoreButtonURLTarget { get; set; }

    public IEnumerable<Guid> FeaturedFlexibleContentGuids { get; set; } = [];

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
