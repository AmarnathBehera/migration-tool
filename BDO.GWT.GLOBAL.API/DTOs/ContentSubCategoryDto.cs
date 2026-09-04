using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.DTOs;

public class ContentSubCategoryDto
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = null!;
}
