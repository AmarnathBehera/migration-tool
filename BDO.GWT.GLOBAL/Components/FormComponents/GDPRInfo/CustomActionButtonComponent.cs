using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: RegisterFormComponent(
    CustomActionButtonComponent.IDENTIFIER,
    typeof(CustomActionButtonComponent),
    "GDPR Action Button"
)]

namespace GWTXbyKWeb.Components.FormComponents.GDPRInfo;

[ComponentAttribute(typeof(CustomActionButtonComponentAttribute))]
public class CustomActionButtonComponent
    : FormComponent<CustomActionButtonProperties, CustomActionButtonClientProperties, string>
{
    public const string IDENTIFIER = "BDO.GdprActionButton";

    private readonly IGDPRUserLookupService gdprUserLookupService;

    public CustomActionButtonComponent(IGDPRUserLookupService gdprUserLookupService)
    {
        this.gdprUserLookupService = gdprUserLookupService;
    }

    public override string ClientComponentName => "@bdo/custom-components/CustomActionButton";

    protected override async Task ConfigureClientProperties(CustomActionButtonClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);
        clientProperties.FieldLabel = clientProperties.Label;
    }

    [FormComponentCommand]
    public Task<GdprCommandResult> FetchTextData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return Task.FromResult(new GdprCommandResult { Success = false, Message = "Email address is empty." });

        var results = gdprUserLookupService.GetContactAsText(data.Trim());
        return Task.FromResult(new GdprCommandResult { Success = true, Message = results });
    }

    [FormComponentCommand]
    public Task<GdprCommandResult> FetchXmlData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return Task.FromResult(new GdprCommandResult { Success = false, Message = "Email address is empty." });

        var results = gdprUserLookupService.GetContactAsXml(data.Trim());
        return Task.FromResult(new GdprCommandResult { Success = true, Message = results });
    }

    [FormComponentCommand]
    public Task<GdprCommandResult> Delete(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return Task.FromResult(new GdprCommandResult { Success = false, Message = "Email address is empty." });

        var deleted = gdprUserLookupService.DeleteContactByEmail(data.Trim());
        return Task.FromResult(new GdprCommandResult
        {
            Success = deleted,
            Message = deleted ? "Contact deleted successfully." : "No contact found to delete."
        });
    }
}

public class GdprCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class CustomActionButtonProperties : FormComponentProperties
{
}

public class CustomActionButtonClientProperties : FormComponentClientProperties<string>
{
    public string FieldLabel { get; set; }
}