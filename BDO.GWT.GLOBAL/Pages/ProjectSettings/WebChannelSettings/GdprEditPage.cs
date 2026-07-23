using BDO.GWT.GLOBAL.Services;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPREditPage : InfoEditPage<GDPRInfo>
{
    private readonly IInfoProvider<GDPRInfo> GDPRInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;
    private readonly IGDPRContactLookupService gdprContactLookupService;
    protected override string ObjectType => GDPRInfo.OBJECT_TYPE;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId
    {
        get
        {
            var existing = GDPRInfoProvider.Get()
                .WhereEquals(nameof(GDPRInfo.GDPRWebChannelSettingID), WebChannelSettingsId)
                .FirstOrDefault();

            if (existing != null)
                return existing.GDPRID;

            if (WebChannelSettingsId <= 0)
                return 0;

            // Auto-create an empty GDPRInfo for this channel so the edit form can load.
            var newGDPRInfo = new GDPRInfo
            {
                GDPRWebChannelSettingID = WebChannelSettingsId
            };
            GDPRInfoProvider.Set(newGDPRInfo);

            return newGDPRInfo.GDPRID;
        }
        set => _ = value;
    }

    public GDPREditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GDPRInfo> GDPRInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService,
        IGDPRContactLookupService gdprContactLookupService)
            : base(formComponentMapper, formDataBinder)
    {
        this.GDPRInfoProvider = GDPRInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
        this.gdprContactLookupService = gdprContactLookupService;
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
    }


    // Loads the GDPR record currently bound to the edit form.
    private GDPRInfo GetCurrentGDPRInfo() => GDPRInfoProvider.Get(ObjectId);

    private async Task<ICommandResponse> BuildFormCommandResponse(bool success, string message)
    {
        var items = await GetFormItems();

        var response = ResponseFrom(new FormSubmissionResult(
            success ? FormSubmissionStatus.ValidationSuccess : FormSubmissionStatus.ValidationFailure)
        {
            Items = await items.OnlyVisible().GetClientProperties(),
        });

        if (success)
        {
            response.AddSuccessMessage(message);
        }
        else
        {
            response.AddErrorMessage(message);
        }

        return response;
    }

    // "Search to Text" button: looks up the contact and stores the plain-text result.
    [PageCommand]
    public async Task<ICommandResponse> FetchTextData()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        gdpr.Results = gdprContactLookupService.GetContactAsText(gdpr.EmailAddress);
        GDPRInfoProvider.Set(gdpr);

        return await BuildFormCommandResponse(true, "Data fetched successfully.");
    }

    // "Search to XML" button: looks up the contact and stores the XML result.
    [PageCommand]
    public async Task<ICommandResponse> FetchXmlData()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        gdpr.Results = gdprContactLookupService.GetContactAsXml(gdpr.EmailAddress);
        GDPRInfoProvider.Set(gdpr);

        return await BuildFormCommandResponse(true, "Data fetched successfully.");
    }

    // "Copy" button: returns the stored results so the client can copy them to the clipboard.
    [PageCommand]
    public Task<ICommandResponse<GdprResultsResult>> Copy()
    {
        var gdpr = GetCurrentGDPRInfo();
        var results = gdpr?.Results ?? string.Empty;

        return Task.FromResult(ResponseFrom(new GdprResultsResult { Results = results }));
    }

    // "Delete" button: clears the stored results.
    [PageCommand]
    public async Task<ICommandResponse> Delete()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        gdpr.Results = string.Empty;
        GDPRInfoProvider.Set(gdpr);

        return await BuildFormCommandResponse(true, "Results deleted.");
    }
}

public class GdprResultsResult
{
    public string Results { get; set; } = string.Empty;
}