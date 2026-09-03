using System.Threading.Tasks;
using Kentico.Xperience.Admin.Base.Forms;
using BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault;

[assembly: RegisterFormComponent(
    GlobalLocationDefaultComponent.IDENTIFIER,
    typeof(GlobalLocationDefaultComponent),
    "Default Setting Text"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault;

[ComponentAttribute(typeof(GlobalLocationDefaultComponentAttribute))]
public class GlobalLocationDefaultComponent
    : FormComponent<GlobalLocationDefaultProperties, GlobalLocationDefaultClientProperties, string>
{
    public const string IDENTIFIER = "BDO.DefaultSetting";

    private readonly GlobalLocationDefaultScriptExecutor _scriptExecutor;

    public GlobalLocationDefaultComponent(GlobalLocationDefaultScriptExecutor scriptExecutor)
    {
        _scriptExecutor = scriptExecutor;
    }

    public override string ClientComponentName => "@bdo/Default-components/BDODefaultSetting";
    protected override async Task ConfigureClientProperties(
        GlobalLocationDefaultClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        if (!string.IsNullOrWhiteSpace(Properties.GlobalSettingName))
        {
            clientProperties.Options = await _scriptExecutor.ExecuteAsync(Properties.GlobalSettingName);
        }
        else
        {
            clientProperties.Options = "";
        }

        clientProperties.FieldLabel = clientProperties.Label;
    }
}