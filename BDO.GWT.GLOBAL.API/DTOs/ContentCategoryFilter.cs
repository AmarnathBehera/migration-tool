using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.DTOs;

public class ContentCategoryFilter
{
    [JsonPropertyName("CategoryCode")]
    public string? CategoryCode { get; set; }

    [JsonPropertyName("SubCategoryIds")]
    public int[]? SubCategoryIds { get; set; }
}
