using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class WebChannelSettingsListingPage : ListingPage
{
    private readonly IInfoProvider<ChannelInfo> channelInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;

    protected override string ObjectType => WebChannelSettingsInfo.OBJECT_TYPE;

    public WebChannelSettingsListingPage(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider,
        IChannelAccessService channelAccessService)
    {
        this.channelInfoProvider = channelInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.seoSettingsInfoProvider = seoSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
        EnsureSettingsListData();
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelDisplayName), "Channel");
        PageConfiguration.AddEditRowAction<WebChannelSettingsEditSection>();

        // ✅ Get accessible IDs for current user
        var accessibleIds = await channelAccessService.GetAccessibleWebChannelSettingsIdsAsync();

        // null = global admin, skip filtering entirely
        if (accessibleIds != null)
        {
            var idList = accessibleIds.ToList();

            // ✅ Correct API: new QueryModifier(Func<ObjectQuery, LoadDataSettings, ObjectQuery>)
            // then .Add() on the IList<QueryModifier>
            PageConfiguration.QueryModifiers.Add(new QueryModifier((query, _) =>
            {
                if (idList.Count!=0)
                {
                    // Show only channels this user has access to
                    return query.WhereIn(
                        nameof(WebChannelSettingsInfo.WebChannelSettingsID),
                        idList);
                }
                else
                {
                    // User has no channel access — return empty result
                    return query.WhereEquals(
                        nameof(WebChannelSettingsInfo.WebChannelSettingsID), 0);
                }
            }));
        }

        await base.ConfigurePage();
    }

    // ── All existing methods below — unchanged ────────────────────────────────

    private void EnsureSettingsListData()
    {
        var channels = channelInfoProvider.Get();
        var webChannelSettings = webChannelSettingsInfoProvider.Get().ToList();
        var seoSettings = seoSettingsInfoProvider.Get().ToList();

        foreach (var channel in channels)
        {
            var currentChannelSettings = webChannelSettings
                .Where(s => s.WebChannelSettingsChannelID.Equals(channel.ChannelID))
                .ToList();

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

    private void EnsureChannelSettingDisplayName(ChannelInfo channel, WebChannelSettingsInfo currentChannelSetting)
    {
        if (currentChannelSetting != null &&
            !channel.ChannelDisplayName.Equals(currentChannelSetting.WebChannelSettingsChannelDisplayName))
        {
            currentChannelSetting.WebChannelSettingsChannelDisplayName = channel.ChannelDisplayName;
            webChannelSettingsInfoProvider.Set(currentChannelSetting);
        }
    }

    private void EnsureSeoSettings(WebChannelSettingsInfo currentChannelSetting, IEnumerable<SEOSettingsInfo> seoSettings)
    {
        int? webChannelSettingsId = currentChannelSetting?.WebChannelSettingsID;
        var currentSeoSettings = seoSettings
            .Where(s => s.SeoSettingsWebChannelSettingID == webChannelSettingsId)
            .ToList();

        if (currentSeoSettings.Count() == 0 && webChannelSettingsId != null)
        {
            seoSettingsInfoProvider.Set(new SEOSettingsInfo
            {
                SeoSettingsWebChannelSettingID = (int)webChannelSettingsId,
                SeoSettingsRobots = string.Empty,
            });
        }
    }
}