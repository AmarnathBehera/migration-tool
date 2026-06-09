namespace BDO.GWT.Core.Data;

public class TabAccordionData : BaseWidgetData
{
    public string? Title { get; set; }

    public string Layout { get; set; } = null!;

    public IEnumerable<Guid> PageGuids { get; set; } = Enumerable.Empty<Guid>();

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;
}
