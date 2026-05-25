using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class EventsWebinarEditPage : InfoEditPage<EventsWebinarsInfo>
{
    private readonly IInfoProvider<EventsWebinarsInfo> EventsWebinarInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId
    {
        get
        {
            var existing = EventsWebinarInfoProvider.Get()
                .WhereEquals(nameof(EventsWebinarsInfo.EventnWebinarWebChannelSettingID), WebChannelSettingsId)
                .FirstOrDefault();

            if (existing != null)
                return existing.EventsWebinarsID;

            if (WebChannelSettingsId <= 0)
                return 0;

            var newEventnwebinarInfo = new EventsWebinarsInfo
            {
                EventnWebinarWebChannelSettingID = WebChannelSettingsId,
                Displaylocationtitleinasidebar = false,
                Minutestotriggercalendarreminder= 0,

            };
            EventsWebinarInfoProvider.Set(newEventnwebinarInfo);

            return newEventnwebinarInfo.EventsWebinarsID;
        }
       
        set => _ = value;
    }

    public EventsWebinarEditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<EventsWebinarsInfo> EventsWebinarInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService)
            : base(formComponentMapper, formDataBinder)
    {
        this.EventsWebinarInfoProvider = EventsWebinarInfoProvider;
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
           
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "eventwebinar";
        await base.ConfigurePage();
    }

    protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
        EventsWebinarsInfo savedInfoObject, ICollection<IFormItem> items)
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