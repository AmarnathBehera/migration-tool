using System.Threading.Tasks;
using BDO.GWT.GLOBAL.Components.FormComponents.FieldGroupSeparator;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(
    FieldGroupSeparatorFormComponent.IDENTIFIER,
    typeof(FieldGroupSeparatorFormComponent),
    "Field group header / separator"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.FieldGroupSeparator;

/// <summary>
/// A visual-only form component intended to act as a field group header and separator in the admin UI.
/// </summary>
[ComponentAttribute(typeof(FieldGroupSeparatorFormComponentAttribute))]
public sealed class FieldGroupSeparatorFormComponent : FormComponent<FieldGroupSeparatorFormComponentClientProperties, string>
{
    public const string IDENTIFIER = "BDO.GWT.Admin.FieldGroupSeparator";

    public override string ClientComponentName => "@bdo/custom-dropdown/FieldGroupSeparator";

    protected override Task ConfigureClientProperties(FieldGroupSeparatorFormComponentClientProperties clientProperties)
    {
        base.ConfigureClientProperties(clientProperties);

        clientProperties.Value ??= string.Empty;

        return Task.CompletedTask;
    }
}
