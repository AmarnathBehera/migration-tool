using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.DTOs;

public class TrendingTopicsCarouselDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public IEnumerable<TrendingTopicsCarouselItemDto> Items { get; set; } = Enumerable.Empty<TrendingTopicsCarouselItemDto>();

    public string TopMargin { get; set; } = null!;

    public string BottomMargin { get; set; } = null!;

    public string? WidgetID { get; set; }

    public string? LearnMoreButtonURLTarget { get; set; } = "_self";
}
