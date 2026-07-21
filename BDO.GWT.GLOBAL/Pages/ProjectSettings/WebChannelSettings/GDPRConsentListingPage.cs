using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPRConsentListingPage : ListingPage
{
    private readonly IInfoProvider<ChannelInfo> channelInfoProvider;
    private readonly IInfoProvider<GDPRInfo> GDPRInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;

    protected override string ObjectType => WebChannelSettingsInfo.OBJECT_TYPE;

    [PageParameter(typeof(StringPageModelBinder))]
    public string EmailAddress { get; set; }

    [PageParameter(typeof(StringPageModelBinder))]
    public string Results { get; set; }

    public GDPRConsentListingPage(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService)
    {
        this.channelInfoProvider = channelInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
        EnsureSettingsListData();
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelDisplayName), "Channel");
        PageConfiguration.AddEditRowAction<WebChannelSettingsEditSection>();

        var accessibleIds = await channelAccessService.GetAccessibleWebChannelSettingsIdsAsync();

        if (accessibleIds != null)
        {
            var idList = accessibleIds.ToList();
            PageConfiguration.QueryModifiers.Add(new QueryModifier((query, _) =>
            {
                if (idList.Count!=0)
                {
                    return query.WhereIn(
                        nameof(WebChannelSettingsInfo.WebChannelSettingsID),
                        idList);
                }
                else
                {
                    return query.WhereEquals(
                        nameof(WebChannelSettingsInfo.WebChannelSettingsID), 0);
                }
            }));
        }

        await base.ConfigurePage();
    }


    private void EnsureSettingsListData()
    {
        var channels = channelInfoProvider.Get();
        var webChannelSettings = webChannelSettingsInfoProvider.Get().ToList();
        var gdprSettings = GDPRInfoProvider.Get().ToList();

        foreach (var channel in channels)
        {
            var currentChannelSettings = webChannelSettings
                .Where(s => s.WebChannelSettingsChannelID.Equals(channel.ChannelID))
                .ToList();

            EnsureChannelSetting(currentChannelSettings, channel);
            var currentChannelSetting = currentChannelSettings.FirstOrDefault();
            EnsureChannelSettingDisplayName(channel, currentChannelSetting);
            EnsureGdprSettings(currentChannelSetting, gdprSettings);
        }
    }

    private void EnsureChannelSetting(List<WebChannelSettingsInfo> currentChannelSettings, ChannelInfo channel)
    {
        if (currentChannelSettings.Count() == 0)
        {
            var newSetting = new WebChannelSettingsInfo
            {
                WebChannelSettingsChannelID = channel.ChannelID,
                WebChannelSettingsChannelDisplayName = channel.ChannelDisplayName,
            };
            webChannelSettingsInfoProvider.Set(newSetting);
            currentChannelSettings.Add(newSetting);
        }
    }

    private void EnsureChannelSettingDisplayName(ChannelInfo channel, WebChannelSettingsInfo currentChannelSetting)
    {
        if (currentChannelSetting != null &&
            !channel.ChannelDisplayName.Equals(currentChannelSetting.WebChannelSettingsChannelDisplayName))
        {
            currentChannelSetting.WebChannelSettingsChannelDisplayName = channel.ChannelDisplayName;
            webChannelSettingsInfoProvider.Set(currentChannelSetting);
        }
    }

    private void EnsureGdprSettings(WebChannelSettingsInfo currentChannelSetting, IEnumerable<GDPRInfo> gdprSettings)
    {
        int? webChannelSettingsId = currentChannelSetting?.WebChannelSettingsID;
        var currentGdprSettings = gdprSettings
            .Where(s => s.GDPRWebChannelSettingID == webChannelSettingsId)
            .ToList();

        if (currentGdprSettings.Count() == 0 && webChannelSettingsId != null)
        {
            GDPRInfoProvider.Set(new GDPRInfo
            {
                GDPRWebChannelSettingID = (int)webChannelSettingsId,
                Consent = string.Empty,
            });
        }
    }
}