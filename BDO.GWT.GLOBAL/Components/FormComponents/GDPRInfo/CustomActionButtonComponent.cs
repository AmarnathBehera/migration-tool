using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: RegisterFormComponent(
    GDPRControlComponent.IDENTIFIER,
    typeof(GDPRControlComponent),
    "GDPR Action Button"
)]

namespace GWTXbyKWeb.Components.FormComponents.GDPRInfo;

[ComponentAttribute(typeof(GDPRControlComponentAttribute))]
public class GDPRControlComponent
    : FormComponent<GDPRControlProperties, GDPRControlClientProperties, string>
{
    public const string IDENTIFIER = "BDO.GdprActionButton";

    private readonly IGDPRUserLookupService gdprUserLookupService;

    public GDPRControlComponent(IGDPRUserLookupService gdprUserLookupService)
    {
        this.gdprUserLookupService = gdprUserLookupService;
    }

    public override string ClientComponentName => "@bdo/custom-components/GDPRControl";

    protected override async Task ConfigureClientProperties(GDPRControlClientProperties clientProperties)
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

        bool deleted = gdprUserLookupService.DeleteContactByEmail(data.Trim());
        if (!deleted)
            return Task.FromResult(new GdprCommandResult { Success = false, Message = $"No personal data found for {data.Trim()}." });

        return Task.FromResult(new GdprCommandResult { Success = true, Message = "Personal data successfully deleted." });
    }
}

public class GdprCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GDPRControlProperties : FormComponentProperties
{
}

public class GDPRControlClientProperties : FormComponentClientProperties<string>
{
    public string FieldLabel { get; set; } = string.Empty;
}