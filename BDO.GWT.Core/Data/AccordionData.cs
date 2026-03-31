using BDO.GWT.Core;

namespace BDO.GWT.Core.Data;

public class AccordionData : BaseWidgetData
{
    public IEnumerable<AccordionItem> AccordionItems { get; set; } = Enumerable.Empty<AccordionItem>();

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public bool UseRichTextContent { get; set; } = true;
}
