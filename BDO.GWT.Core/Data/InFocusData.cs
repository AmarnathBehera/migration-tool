using System;
using System.Collections.Generic;

namespace BDO.GWT.Core.Data;

public class InFocusData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public IEnumerable<Guid> PageGuids { get; set; } = [];

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
