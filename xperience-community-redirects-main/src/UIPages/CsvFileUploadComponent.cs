using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.Redirects.UIPages;

[assembly: RegisterFormComponent(
    CsvFileUploadComponent.IDENTIFIER,
    typeof(CsvFileUploadComponent),
    "CSV file upload")]

namespace XperienceCommunity.Redirects.UIPages;

[ComponentAttribute(typeof(CsvFileUploadComponentAttribute))]
internal sealed class CsvFileUploadComponent
    : FormComponent<CsvFileUploadComponentProperties, CsvFileUploadComponentClientProperties, string>
{
    public const string IDENTIFIER = "XperienceCommunity.Redirects.CsvFileUpload";

    public override string ClientComponentName => "@bdo/custom-components/CsvFileUpload";

    protected override Task ConfigureClientProperties(CsvFileUploadComponentClientProperties clientProperties)
    {
        clientProperties.Value ??= string.Empty;
        return base.ConfigureClientProperties(clientProperties);
    }
}
