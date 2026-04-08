using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class WebChannelSettingsListingPage : ListingPage
{
    private readonly IInfoProvider<ChannelInfo> channelInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider;

    protected override string ObjectType => WebChannelSettingsInfo.OBJECT_TYPE;

    public WebChannelSettingsListingPage(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider)
    {
        this.channelInfoProvider = channelInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.seoSettingsInfoProvider = seoSettingsInfoProvider;

        EnsureSettingsListData();
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(
                WebChannelSettingsInfo.WebChannelSettingsChannelDisplayName), "Channel");

        PageConfiguration.AddEditRowAction<WebChannelSettingsEditSection>();

        await base.ConfigurePage();
    }
    private void EnsureSettingsListData()
    {
        var channels = channelInfoProvider.Get();
        var webChannelSettings = webChannelSettingsInfoProvider.Get().ToList();
        var seoSettings = seoSettingsInfoProvider.Get().ToList();

        foreach (var channel in channels)
        {
            var currentChannelSettings = webChannelSettings.Where(setting => setting.WebChannelSettingsChannelID.Equals(channel.ChannelID)).ToList();

            EnsureChannelSetting(currentChannelSettings, channel);

            var currentChannelSetting = currentChannelSettings.FirstOrDefault();

            EnsureChannelSettingDisplayName(channel, currentChannelSetting);

            EnsureSeoSettings(currentChannelSetting, seoSettings);
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

    private void EnsureChannelSettingDisplayName(ChannelInfo channel, WebChannelSettingsInfo? currentChannelSetting)
    {
        if (currentChannelSetting != null && !channel.ChannelDisplayName.Equals(currentChannelSetting.WebChannelSettingsChannelDisplayName))
        {
            currentChannelSetting.WebChannelSettingsChannelDisplayName = channel.ChannelDisplayName;
            webChannelSettingsInfoProvider.Set(currentChannelSetting);
        }
    }

     private void EnsureSeoSettings(WebChannelSettingsInfo? currentChannelSetting, IEnumerable<SEOSettingsInfo> seoSettings)
    {
        int? webChannelSettingsId = currentChannelSetting?.WebChannelSettingsID;

        var currentSeoSettings = seoSettings
            .Where(setting => setting.SeoSettingsWebChannelSettingID == webChannelSettingsId).ToList();

        if (currentSeoSettings.Count() == 0 && webChannelSettingsId != null)
        {
            var newSeoSetting = new SEOSettingsInfo
            {
                SeoSettingsWebChannelSettingID = (int)webChannelSettingsId,
                SeoSettingsRobots = string.Empty,
            };

            seoSettingsInfoProvider.Set(newSeoSetting);
        }
    }
}
