using System.Threading.Tasks;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(
    CustomDropdownComponent.IDENTIFIER,
    typeof(CustomDropdownComponent),
    "SQL Dropdown"
)]

// ✅ [ComponentAttribute] links the attribute class to this component
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

    // Must match the key in entries.ts
    public override string ClientComponentName => "@bdo/custom-dropdown/BDOSQLDropdown";
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

        // Explicitly pass the label from the base class
        clientProperties.FieldLabel = clientProperties.Label;
    }
}