using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.DTOs;


public class ContentDto
{
    [JsonPropertyName("InsightsSearchURL")]
    public string? InsightsSearchURL { get; set; } = null!;

    [JsonPropertyName("Insights")]
    public List<InsightContentDto> Insights { get; set; } = null!;
}
