using BDO.GWT.CustomReportingModule;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Microsoft.Extensions.Localization;
[assembly: UIPage(
    parentType: typeof(ReportingApplicationPage),
    slug: "reporting-application-channel-settings",
    uiPageType: typeof(BDO.GWT.CustomReportingModule.ReportingApp.ReportingApplicationChannelSettingsListingPage),
    name: "Reporting Application Channel settings",
    templateName: TemplateNames.LISTING,
    order: 0)]

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class ReportingApplicationChannelSettingsListingPage : ListingPage
    {
        private readonly IInfoProvider<ChannelInfo> channelInfoProvider;
        private readonly IInfoProvider<ReportingChannelSettingInfo> reportingChannelSettingInfoProvider;
        private readonly IStringLocalizer<SharedResources> stringLocalizer;

        protected override string ObjectType => ReportingChannelSettingInfo.OBJECT_TYPE;

        public ReportingApplicationChannelSettingsListingPage(
            IInfoProvider<ChannelInfo> channelInfoProvider,
            IInfoProvider<ReportingChannelSettingInfo> reportingChannelSettingInfoProvider,
            IStringLocalizer<SharedResources> stringLocalizer) : base()
        {
            this.channelInfoProvider = channelInfoProvider;
            this.reportingChannelSettingInfoProvider = reportingChannelSettingInfoProvider;
            this.stringLocalizer = stringLocalizer;
            EnsureSettingsListData();
        }

        private void EnsureSettingsListData()
        {
            var channels = channelInfoProvider.Get();
            var reportingChannelSettings = reportingChannelSettingInfoProvider.Get().ToList();

            foreach (var channel in channels)
            {
                var currentChannelSettings = reportingChannelSettings.Where(setting => setting.ReportingChannelSettingID.Equals(channel.ChannelID)).ToList();

                EnsureChannelSetting(currentChannelSettings, channel);

                var currentChannelSetting = currentChannelSettings.FirstOrDefault();

                EnsureChannelSettingDisplayName(channel, currentChannelSetting);

            }
        }
        private void EnsureChannelSetting(List<ReportingChannelSettingInfo> currentChannelSettings, ChannelInfo channel)
        {
            if (currentChannelSettings.Count == 0)
            {
                var newSetting = new ReportingChannelSettingInfo
                {
                    ReportingChannelSettingsUId = channel.ChannelID,
                    ReportingChannelSettingsDisplayName = channel.ChannelDisplayName,
                };
                reportingChannelSettingInfoProvider.Set(newSetting);
                currentChannelSettings.Add(newSetting);
            }
        }

        private void EnsureChannelSettingDisplayName(ChannelInfo channel, ReportingChannelSettingInfo? currentChannelSetting)
        {
            if (currentChannelSetting != null && !channel.ChannelDisplayName.Equals(currentChannelSetting.ReportingChannelSettingsDisplayName))
            {
                currentChannelSetting.ReportingChannelSettingsDisplayName = channel.ChannelDisplayName;
                reportingChannelSettingInfoProvider.Set(currentChannelSetting);
            }
        }

        public override async Task ConfigurePage()
        {
            PageConfiguration.ColumnConfigurations
                         .AddColumn(nameof(
                            ReportingChannelSettingInfo.ReportingChannelSettingsDisplayName), stringLocalizer["ReportingChannel"]);

            PageConfiguration.AddEditRowAction<ReportingApplicationChannelSettingsEditSection>();

            await base.ConfigurePage();
        }
    }
}