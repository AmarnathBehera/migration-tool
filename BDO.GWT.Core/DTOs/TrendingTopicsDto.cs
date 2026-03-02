using System.Collections.Generic;


namespace BDO.GWT.Core;

public class TrendingTopicsDto
{
    public string Heading { get; set; } = null!;

    public IEnumerable<TrendingTopicTabDto> Tabs { get; set; } = new List<TrendingTopicTabDto>();
}
