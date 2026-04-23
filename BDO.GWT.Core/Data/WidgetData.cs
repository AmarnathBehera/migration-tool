using BDO.GWT.Core;
using System.Text.Json.Serialization;

namespace BDO.GWT.Core;

public class WidgetData<TWidgetProperties> : WidgetBaseData
    where TWidgetProperties : IWidgetPropertiesData
{
    [JsonIgnore]
    public List<WidgetVariantData<TWidgetProperties>> Variants
    {
        get => (List<WidgetVariantData<TWidgetProperties>>?)VariantsObject ?? [];
        set => VariantsObject = value;
    }
}

public abstract class WidgetBaseData
{
    public Guid Identifier { get; set; }

    [JsonPropertyName("type")]
    public string TypeIdentifier { get; set; } = null!;

    [JsonPropertyName("variants")]
    public object? VariantsObject { get; protected set; }
}
