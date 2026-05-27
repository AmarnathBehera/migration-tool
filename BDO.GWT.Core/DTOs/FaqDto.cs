namespace BDO.GWT.Core.DTOs;

public class FaqDto
{
    public string? Title { get; set; }

    public string Layout { get; set; } = null!;

    public FaqPageDto[] Topics { get; set; } = [];

    public string HoverColour { get; set; } = null!;

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }
}
