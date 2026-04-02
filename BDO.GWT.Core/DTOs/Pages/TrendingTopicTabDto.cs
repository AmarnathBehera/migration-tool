using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs; 

public class TrendingTopicTabDto
{
    public string Title { get; set; } = null!;

    public string ImageURL { get; set; } = null!;

    public string ImageAltText { get; set; } = null!;

    public List<TrendingTopicItemDto> Items { get; set; } = new();

    public string? ReadMoreLink { get; set; }
}
