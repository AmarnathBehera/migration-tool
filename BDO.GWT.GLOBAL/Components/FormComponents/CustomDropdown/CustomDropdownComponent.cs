using System.Threading.Tasks;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

[assembly: RegisterFormComponent(
    CustomDropdownComponent.IDENTIFIER,
    typeof(CustomDropdownComponent),
    "Query dropdown"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

[ComponentAttribute(typeof(CustomDropdownComponentAttribute))]
public class CustomDropdownComponent
    : FormComponent<CustomDropdownProperties, CustomDropdownClientProperties, string>
{
    public const string IDENTIFIER = "BDO.SQLDropdown";

    private readonly DropdownScriptExecutor _scriptExecutor;

    public CustomDropdownComponent(DropdownScriptExecutor scriptExecutor)
    {
        _scriptExecutor = scriptExecutor;
    }

    public override string ClientComponentName => "@bdo/custom-components/BDOSQLDropdown";
    protected override async Task ConfigureClientProperties(
        CustomDropdownClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        if (!string.IsNullOrWhiteSpace(Properties.Script))
        {
            clientProperties.Options = await _scriptExecutor.ExecuteAsync(Properties.Script);
        }
        else
        {
            clientProperties.Options = [];
        }

        clientProperties.FieldLabel = clientProperties.Label;
    }
}