using System;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.DTOs;


public class InsightContentDto
{
    [JsonPropertyName("GUID")]
    public Guid GUID { get; set; }

    [JsonPropertyName("PageType")]
    public string PageType { get; set; } = null!;

    [JsonPropertyName("PageSubType")]
    public string PageSubType { get; set; } = null!;

    [JsonPropertyName("Title_Full")]
    public string TitleFull { get; set; } = null!;

    [JsonPropertyName("Title_TwoThirds")]
    public string TitleTwoThirds { get; set; } = null!;

    [JsonPropertyName("Title_OneThird")]
    public string TitleOneThird { get; set; } = null!;

    [JsonPropertyName("Summary_Full")]
    public string SummaryFull { get; set; } = null!;

    [JsonPropertyName("Summary_TwoThirds")]
    public string SummaryTwoThirds { get; set; } = null!;

    [JsonPropertyName("Summary_OneThird")]
    public string SummaryOneThird { get; set; } = null!;

    [JsonPropertyName("ImageURL_Full")]
    public string ImageUrlFull { get; set; } = null!;

    [JsonPropertyName("ImageURL_TwoThirds")]
    public string ImageUrlTwoThirds { get; set; } = null!;

    [JsonPropertyName("ImageURL_OneThird")]
    public string ImageUrlOneThird { get; set; } = null!;

    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("DisplayDate")]
    public string DisplayDate { get; set; } = null!;

    [JsonPropertyName("URL")]
    public string URL { get; set; } = null!;
}
