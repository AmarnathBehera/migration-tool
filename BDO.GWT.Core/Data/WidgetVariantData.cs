namespace BDO.GWT.Core;

public class WidgetVariantData<TWidgetProperties> : WidgetVariantBaseData
where TWidgetProperties : IWidgetPropertiesData
{
public TWidgetProperties Properties { get; set; } = default!;
}

public abstract class WidgetVariantBaseData
{
public Guid Identifier { get; set; }

public string? Name { get; set; }
}