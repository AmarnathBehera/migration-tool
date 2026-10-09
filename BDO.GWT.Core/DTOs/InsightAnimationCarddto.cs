

namespace BDO.GWT.Core.DTOs;
public class InsightAnimationCardDto 
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<InsightAnimationPageDto> Items { get; set; } = null!;

    public LinkItemDto? SeeAllLink { get; set; }

    public string? OpenInNewTab { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

}