
using BDO.GWT.Core;

namespace BDO.GWT.Core.Data;

public class InsightAnimationCardData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public List<Guid> InsightCards { get; set; } = new List<Guid>();

    public string? openInNewTab { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
