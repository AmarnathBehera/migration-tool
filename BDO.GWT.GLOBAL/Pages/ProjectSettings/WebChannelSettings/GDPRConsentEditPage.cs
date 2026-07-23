using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPRConsentEditPage : InfoEditPage<GDPRInfo>
{
    private readonly IInfoProvider<GDPRInfo> GDPRInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;

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

            var newGDPRInfo = new GDPRInfo
            {
                GDPRWebChannelSettingID = WebChannelSettingsId,
                Consent = ""
            };
            GDPRInfoProvider.Set(newGDPRInfo);

            return newGDPRInfo.GDPRID;
        }
        set => _ = value;
    }

    public GDPRConsentEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GDPRInfo> GDPRInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService)
            : base(formComponentMapper, formDataBinder)
    {
        this.GDPRInfoProvider = GDPRInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
    }

    private string WebChannelSettingsDisplayName =>
        webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsID), WebChannelSettingsId)
            .FirstOrDefault()?
            .WebChannelSettingsChannelDisplayName ?? "Web channel settings";

    public override async Task ConfigurePage()
    {
        bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
        if (!canAccess)
        {
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "gdprconsent";
        PageConfiguration.Headline = "GDPR Consent";
        await base.ConfigurePage();
    }

    protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
        GDPRInfo savedInfoObject,
        ICollection<IFormItem> items)
    {
        var result = new EditPageSuccessFormSubmissionResult()
        {
            Items = await items.OnlyVisible().GetClientProperties(),
            ObjectDisplayName = WebChannelSettingsDisplayName,
            ObjectId = WebChannelSettingsId,
            RefetchAll = RefetchAll
        };

        return ResponseFrom(result)
            .AddSuccessMessage(LocalizationService?.GetString("base.forms.saved"));
    }
}