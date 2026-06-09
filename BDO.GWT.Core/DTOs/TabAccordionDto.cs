using System;

namespace BDO.GWT.Core.DTOs;

public class TabAccordionDto
{
    public string? Title { get; set; }

    public string Layout { get; set; } = null!;

    public TabAccordionItemDto[] Items { get; set; } = Array.Empty<TabAccordionItemDto>();

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public bool UseRichTextContent { get; set; } = true;
}
