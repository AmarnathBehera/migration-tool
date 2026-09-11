using BDO.GWT.GLOBAL.Services;
using CMS.DataEngine;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPRInfoPage : InfoEditPage<GDPRInfo>
{
    private readonly IInfoProvider<GDPRInfo> gdprInfoProvider;
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

    // The single field on this page: renders the custom widget
    // (email input + Search/Delete buttons + results area, all client-side).
    [GDPRControlComponentAttribute(Label = "")]
    public string GdprLookup { get; set; } = string.Empty;

    public GDPRInfoPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GDPRInfo> gdprInfoProvider,
        IChannelAccessService channelAccessService,
        IGDPRUserLookupService gdprUserLookupService)
            : base(formComponentMapper, formDataBinder)
    {
        this.gdprInfoProvider = gdprInfoProvider;
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

        await base.ConfigurePage();

        PageConfiguration.SubmitConfiguration.Visible = false;
    }

    // Loads the GDPR record currently bound to this channel.
    private GDPRInfo GetCurrentGDPRInfo() => gdprInfoProvider.Get()
        .WhereEquals(nameof(GDPRInfo.GDPRWebChannelSettingID), WebChannelSettingsId)
        .FirstOrDefault();

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

    // "Search to Text" — looks up the contact and returns the plain-text result.
    [PageCommand]
    public async Task<ICommandResponse> FetchTextData(string emailAddress)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        var results = gdprUserLookupService.GetContactAsText(emailAddress.Trim());

        return await BuildFormCommandResponse(true, results);
    }

    // "Search to XML" — looks up the contact and returns the XML result.
    [PageCommand]
    public async Task<ICommandResponse> FetchXmlData(string emailAddress)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        var results = gdprUserLookupService.GetContactAsXml(emailAddress.Trim());

        return await BuildFormCommandResponse(true, "Data fetched successfully.", results);
    }

    // "Delete" — deletes the contact by email.
    [PageCommand]
    public async Task<ICommandResponse> Delete(string emailAddress)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
            return await BuildFormCommandResponse(false, "GDPR record not found.");

        if (string.IsNullOrWhiteSpace(emailAddress))
            return await BuildFormCommandResponse(false, "Email address is empty.");

        if (!gdprUserLookupService.DeleteContactByEmail(emailAddress.Trim()))
            return await BuildFormCommandResponse(false, "No contact found to delete.");

        return await BuildFormCommandResponse(true, "Contact deleted successfully.");
    }
}