using BDO.GWT.Core;

namespace BDO.GWT.Domain.Core.Data.PageBuilder.Widgets;

public class AccordionWidgetPropertiesData : BaseWidgetPropertiesData
{
    public required string Heading { get; set; }

    public required string Content { get; set; }

    public string? DeepLinkID { get; set; }

    public required string TopMargin { get; set; }

    public required string BottomMargin { get; set; }
}

public class AccordionWidgetData : WidgetData<AccordionWidgetPropertiesData>;
