using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: RegisterFormComponent(
    BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData.FetchGlobalSettingFieldDataComponent.IDENTIFIER,
    typeof(BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData.FetchGlobalSettingFieldDataComponent),
    "Fetch global setting field data"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

[ComponentAttribute(typeof(FetchGlobalSettingFieldDataComponentAttribute))]
public class FetchGlobalSettingFieldDataComponent
    : FormComponent<FetchGlobalSettingFieldDataProperties, FetchGlobalSettingFieldDataClientProperties, string>
{
    public const string IDENTIFIER = "BDO.FetchGlobalSettingFieldData";

    private readonly IFetchGlobalSettingFieldDataProvider _fieldDataProvider;

    public FetchGlobalSettingFieldDataComponent(IFetchGlobalSettingFieldDataProvider fieldDataProvider)
    {
        _fieldDataProvider = fieldDataProvider;
    }

    public override string ClientComponentName => "@bdo/custom-components/BDOFetchGlobalSettingFieldData";

    protected override async Task ConfigureClientProperties(FetchGlobalSettingFieldDataClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        clientProperties.Value = await _fieldDataProvider.GetFieldValueAsync(Properties.FieldName);
        clientProperties.FieldLabel = clientProperties.Label;
    }
}
