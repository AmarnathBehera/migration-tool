using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.DTOs;
public class FlexibleContentDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<FlexibleContentItemDto> Items { get; set; } = Enumerable.Empty<FlexibleContentItemDto>();

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public string? LearnMoreButtonURLTarget { get; set; } = "_self";

}
