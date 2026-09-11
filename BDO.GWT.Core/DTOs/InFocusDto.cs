namespace BDO.GWT.Core.DTOs;

public class InFocusDto
{
    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public InFocusTopicDto[] Topics { get; set; } = [];

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }
}
