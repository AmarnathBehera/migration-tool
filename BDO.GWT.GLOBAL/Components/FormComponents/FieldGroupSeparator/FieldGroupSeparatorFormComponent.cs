using System.Threading.Tasks;
using BDO.GWT.GLOBAL.Components.FormComponents.FieldGroupSeparator;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(
    FieldGroupSeparatorFormComponent.IDENTIFIER,
    typeof(FieldGroupSeparatorFormComponent),
    "Form section header"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.FieldGroupSeparator;

[ComponentAttribute(typeof(FieldGroupSeparatorFormComponentAttribute))]
public sealed class FieldGroupSeparatorFormComponent
    : FormComponent<FieldGroupSeparatorFormComponentProperties, FieldGroupSeparatorFormComponentClientProperties, string>
{
    public const string IDENTIFIER = "BDO.GWT.Admin.FieldGroupSeparator";

    public override string ClientComponentName => "@bdo/custom-components/FieldGroupSeparator";

    protected override async Task ConfigureClientProperties(FieldGroupSeparatorFormComponentClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        clientProperties.Value ??= string.Empty;
    }
}
