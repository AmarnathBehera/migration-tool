using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public abstract class BaseSchemaMarkupDto
{
    /// <summary>
    /// The json serializer options
    /// </summary>
    [JsonIgnore]
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(
            UnicodeRanges.BasicLatin,
            UnicodeRanges.Latin1Supplement,
            UnicodeRanges.Cyrillic,
            UnicodeRanges.CyrillicSupplement,
            UnicodeRanges.GreekandCoptic,
            UnicodeRanges.Hebrew, 
            UnicodeRanges.Armenian,
            UnicodeRanges.Georgian,
            UnicodeRanges.Arabic) // To support various languages/chars in schema markup. More can be added if needed.
    };

    /// <summary>
    /// The context
    /// </summary>
    [JsonPropertyName("@context")]
    [JsonInclude]
    public readonly string Context = "https://schema.org";

    /// <summary>
    /// The schema markup type. E.g. Person, Document, etc.
    /// </summary>
    /// <value>
    /// The schema markup type.
    /// </value>
    [JsonPropertyName("@type")]
    public string Type => this.GetSchemaMarkupType();

    /// <summary>
    /// Gets the schema markup type. E.g. Person, Document, etc.
    /// </summary>
    /// <value>
    /// The schema markup type.
    /// </value>
    protected abstract string GetSchemaMarkupType();

    /// <summary>
    /// Converts object to the ld+json script.
    /// </summary>
    /// <returns>The ld+json script.</returns>
    public string ToSchemaMarkupJsString()
    {
        var serializedString = JsonSerializer.Serialize(this, this.GetType(), JsonSerializerOptions);

        var result = $"\n\t<script type=\"application/ld+json\">\n{serializedString}\n\t</script>";
        return result;
    }
}
