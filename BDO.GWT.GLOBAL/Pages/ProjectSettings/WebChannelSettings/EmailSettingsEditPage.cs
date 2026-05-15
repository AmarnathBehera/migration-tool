using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class EmailEditPage : InfoEditPage<EmailInfo>
{
    private readonly IInfoProvider<EmailInfo> EmailInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId
    {
        get
        {
            var existing = EmailInfoProvider.Get()
                .WhereEquals(nameof(EmailInfo.EmailSettingsWebChannelSettingID), WebChannelSettingsId)
                .FirstOrDefault();

            if (existing != null)
                return existing.EmailID;

            if (WebChannelSettingsId <= 0)
                return 0;

            // Auto-create an empty EmailInfo for this channel so the edit form can load.
            var newEmailInfo = new EmailInfo
            {
                EmailSettingsWebChannelSettingID = WebChannelSettingsId,
                DisplayName = "Email",
                CodeName = $"email_settings_{WebChannelSettingsId}",
                FromEmail = string.Empty
            };
            EmailInfoProvider.Set(newEmailInfo);

            return newEmailInfo.EmailID;
        }
        // ObjectId is derived from WebChannelSettingsId; the setter is required
        // by the base contract but the supplied value is intentionally ignored.
        set => _ = value;
    }

    public EmailEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<EmailInfo> EmailInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService)
            : base(formComponentMapper, formDataBinder)
    {
        this.EmailInfoProvider = EmailInfoProvider;
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
        // ✅ Guard: block access if user has no permission for this channel
        bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
        if (!canAccess)
        {
            // ✅ Correct way to block in XbyK — hide the submit button
            // and show nothing by returning before base.ConfigurePage()
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "emailedit";
        await base.ConfigurePage();
    }

    protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
        EmailInfo savedInfoObject, ICollection<IFormItem> items)
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