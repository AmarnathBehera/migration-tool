using BDO.GWT.GLOBAL.Services;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPRInfoPage : InfoEditPage<GDPRInfo>
{
    private readonly IInfoProvider<GDPRInfo> gdprInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;
    private readonly IGDPRUserLookupService gdprUserLookupService;
    protected override string ObjectType => GDPRInfo.OBJECT_TYPE;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId
    {
        get
        {
            var existing = gdprInfoProvider.Get()
                .WhereEquals(nameof(GDPRInfo.GDPRWebChannelSettingID), WebChannelSettingsId)
                .FirstOrDefault();

            if (existing != null)
                return existing.GDPRID;

            if (WebChannelSettingsId <= 0)
                return 0;

            // Auto-create an empty GDPRInfo for this channel so the edit form can load.
            var newGDPRInfo = new GDPRInfo { GDPRWebChannelSettingID = WebChannelSettingsId.ToString() };
            gdprInfoProvider.Set(newGDPRInfo);

            return newGDPRInfo.GDPRID;
        }
        set => _ = value;
    }

    public GDPRInfoPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GDPRInfo> gdprInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService,
        IGDPRUserLookupService gdprUserLookupService)
            : base(formComponentMapper, formDataBinder)
    {
        this.gdprInfoProvider = gdprInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
        this.gdprUserLookupService = gdprUserLookupService;
    }

    public override async Task ConfigurePage()
    {
        bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
        if (!canAccess)
        {
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "gdprdisplay";
        PageConfiguration.Headline = "GDPR Display";
        PageConfiguration.SubmitConfiguration.Visible = false;
        GdprButtonConfiguration.AddCommandActions(PageConfiguration);

        await base.ConfigurePage();

        // Keep this page command-only (no Save button).
        PageConfiguration.SubmitConfiguration.Visible = false;
    }


    // Loads the GDPR record currently bound to this channel.
    private GDPRInfo GetCurrentGDPRInfo() => gdprInfoProvider.Get()
        .WhereEquals(nameof(GDPRInfo.GDPRWebChannelSettingID), WebChannelSettingsId)
        .FirstOrDefault();

    // Reads a submitted (unsaved) form field value sent with the command.
    private static string GetFormValue(FormSubmissionCommandArguments args, params string[] fieldNames)
    {
        if (args?.Data is null)
            return string.Empty;

        foreach (var fieldName in fieldNames)
        {
            var match = args.Data.FirstOrDefault(kvp => string.Equals(kvp.Key, fieldName, StringComparison.OrdinalIgnoreCase));
            var value = ConvertToString(match.Value);
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static string ConvertToString(object value) => value switch
    {
        null => string.Empty,
        string s => s,
        JsonElement { ValueKind: JsonValueKind.String } je => je.GetString() ?? string.Empty,
        JsonElement je => je.ToString(),
        _ => value.ToString() ?? string.Empty
    };

    private async Task<ICommandResponse> BuildFormCommandResponse(bool success, string message, string results = null)
    {
        var items = await GetFormItems();

        var response = ResponseFrom(new FormSubmissionResult(
            success ? FormSubmissionStatus.ValidationSuccess : FormSubmissionStatus.ValidationFailure)
        {
            Items = await items.OnlyVisible().GetClientProperties(),
        });

        if (success)
            response.AddSuccessMessage(results ?? message);
        else
            response.AddErrorMessage(message);

        return response;
    }

    [PageCommand]
    public async Task<ICommandResponse> SendValue(FormSubmissionCommandArguments args)
    {
        var emailAddress = GetFormValue(args, "EmailAddress", "Email");
        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        return await BuildFormCommandResponse(true, "Value received.", emailAddress);
    }

    // "Search to Text" button: looks up the contact and stores the plain-text result.
    [PageCommand]
    public async Task<ICommandResponse> FetchTextData(FormSubmissionCommandArguments args)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        var emailAddress = GetFormValue(args, "EmailAddress", "Email");
        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        var results = gdprUserLookupService.GetContactAsText(emailAddress);

        return await BuildFormCommandResponse(true, "Data fetched successfully.", results);
    }

    // "Search to XML" button: looks up the contact and stores the XML result.
    [PageCommand]
    public async Task<ICommandResponse> FetchXmlData(FormSubmissionCommandArguments args)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        var emailAddress = GetFormValue(args, "EmailAddress", "Email");
        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        var results = gdprUserLookupService.GetContactAsXml(emailAddress);

        return await BuildFormCommandResponse(true, "Data fetched successfully.", results);
    }

    // "Copy" button: returns the stored results as a success message.
    [PageCommand]
    public async Task<ICommandResponse> Copy(FormSubmissionCommandArguments args)
    {
        var results = GetFormValue(args, "Results");

        if (string.IsNullOrWhiteSpace(results))
            return await BuildFormCommandResponse(false, "No results available to copy.");

        return await BuildFormCommandResponse(true, results);
    }

    // "Delete" button: clears the stored results.
    [PageCommand]
    public async Task<ICommandResponse> Delete(FormSubmissionCommandArguments args)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        var emailAddress = GetFormValue(args, "EmailAddress", "Email");
        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        if (!gdprUserLookupService.DeleteContactByEmail(emailAddress))
            return await BuildFormCommandResponse(false, "No contact found to delete.");

        return await BuildFormCommandResponse(true, "Contact deleted successfully.");
    }
}

public sealed class GdprCommandData
{
    public string EmailAddress { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
}

