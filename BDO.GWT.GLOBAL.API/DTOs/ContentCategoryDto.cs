using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.DTOs;

public class ContentCategoryDto
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("Code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("SubCategories")]
    public List<ContentSubCategoryDto> SubCategories { get; set; } = null!;
}
