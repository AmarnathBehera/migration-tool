using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Kentico.Xperience.Admin.Base.Forms;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("BDO.GWT.GLOBAL.Tests")]
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
    public Task<GdprCommandResult> FetchTextData(string data) =>
        Task.FromResult(GdprCommandHandler.HandleFetchText(gdprUserLookupService, data));

    [FormComponentCommand]
    public Task<GdprCommandResult> FetchXmlData(string data) =>
        Task.FromResult(GdprCommandHandler.HandleFetchXml(gdprUserLookupService, data));

    [FormComponentCommand]
    public Task<GdprCommandResult> Delete(string data) =>
        Task.FromResult(GdprCommandHandler.HandleDelete(gdprUserLookupService, data));
}

// Extracted so command logic is testable without Kentico's IoC container.
internal static class GdprCommandHandler
{
    internal static GdprCommandResult HandleFetchText(IGDPRUserLookupService service, string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return new GdprCommandResult { Success = false, Message = "Email address is empty." };

        return new GdprCommandResult { Success = true, Message = service.GetContactAsText(data.Trim()) };
    }

    internal static GdprCommandResult HandleFetchXml(IGDPRUserLookupService service, string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return new GdprCommandResult { Success = false, Message = "Email address is empty." };

        return new GdprCommandResult { Success = true, Message = service.GetContactAsXml(data.Trim()) };
    }

    internal static GdprCommandResult HandleDelete(IGDPRUserLookupService service, string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return new GdprCommandResult { Success = false, Message = "Email address is empty." };

        bool deleted = service.DeleteContactByEmail(data.Trim());
        if (!deleted)
            return new GdprCommandResult { Success = false, Message = $"No personal data found for {data.Trim()}." };

        return new GdprCommandResult { Success = true, Message = "Personal data successfully deleted." };
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
