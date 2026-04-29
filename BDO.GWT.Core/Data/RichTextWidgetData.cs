namespace BDO.GWT.Core;

public class RichTextWidgetPropertiesData : IWidgetPropertiesData
{
    public required string Content { get; set; }

    public string? WidgetID { get; set; }

    public required string TopMargin { get; set; }

    public required string BottomMargin { get; set; }
}

public class RichTextWidgetData : WidgetData<RichTextWidgetPropertiesData>;
