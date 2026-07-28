using BDO.GWT.GLOBAL.Components.FormComponents.EmailLookup;
using BDO.GWT.GLOBAL.Services;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

[assembly: RegisterFormComponent(
    EmailLookupFormComponent.IDENTIFIER,
    typeof(EmailLookupFormComponent),
    "Email lookup"
)]

namespace BDO.GWT.GLOBAL.Components.FormComponents.EmailLookup;

[ComponentAttribute(typeof(EmailLookupFormComponentAttribute))]
public class EmailLookupFormComponent
    : FormComponent<EmailLookupFormComponentProperties, EmailLookupFormComponentClientProperties, string>
{
    public const string IDENTIFIER = "BDO.EmailLookup";

    private readonly IGDPRContactLookupService gdprContactLookupService;

    public EmailLookupFormComponent(IGDPRContactLookupService gdprContactLookupService)
    {
        this.gdprContactLookupService = gdprContactLookupService;
    }

    public override string ClientComponentName => "@bdo/custom-components/EmailLookupFormComponent";

    protected override async Task ConfigureClientProperties(EmailLookupFormComponentClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        clientProperties.LinkText = nameof(FetchTextData);
        clientProperties.LinkXml = nameof(FetchXmlData);
        clientProperties.LinkDelete = nameof(Delete);
        clientProperties.LinkCopy = nameof(Copy);
        clientProperties.Email = Properties.Email;
        clientProperties.Value ??= string.Empty;
        clientProperties.Results = clientProperties.Value;
    }

    [FormComponentCommand]
    public Task<ICommandResponse<EmailLookupCommandResult>> FetchTextData(EmailLookupCommandArguments args)
    {
        var results = gdprContactLookupService.GetContactAsText(args.Email);

        return Task.FromResult(BuildCommandResponse(results)
            .AddSuccessMessage("Data fetched successfully."));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<EmailLookupCommandResult>> FetchXmlData(EmailLookupCommandArguments args)
    {
        var results = gdprContactLookupService.GetContactAsXml(args.Email);

        return Task.FromResult(BuildCommandResponse(results)
            .AddSuccessMessage("Data fetched successfully."));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<EmailLookupCommandResult>> Delete(EmailLookupCommandArguments args)
    {
        if (!gdprContactLookupService.DeleteContactByEmail(args.Email))
        {
            return Task.FromResult(BuildCommandResponse("No contact found to delete.")
                .AddErrorMessage("No contact found to delete."));
        }

        return Task.FromResult(BuildCommandResponse("Contact deleted successfully.")
            .AddSuccessMessage("Contact deleted successfully."));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<EmailLookupCommandResult>> Copy(EmailLookupCommandArguments args)
    {
        return Task.FromResult(BuildCommandResponse(args.Results));
    }

    private ICommandResponse<EmailLookupCommandResult> BuildCommandResponse(string results)
    {
        return ResponseFrom(new EmailLookupCommandResult
        {
            Results = results ?? string.Empty,
        });
    }
}

public class EmailLookupCommandArguments
{
    public string Email { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
}

public class EmailLookupCommandResult
{
    public string Results { get; set; } = string.Empty;
}
