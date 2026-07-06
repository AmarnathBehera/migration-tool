
namespace BDO.GWT.Core.Data;

public class RelatedContactsData : BaseWidgetData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? OrderCriteria { get; set; }

    public bool IsDescendingOrder { get; set; }

    public bool ShowOffice { get; set; }

    public string? SeeAllLinkText { get; set; }

    public string? SeeAllLinkUrl { get; set; }

    public string? SeeAllLinkURLTarget { get; set; }

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
