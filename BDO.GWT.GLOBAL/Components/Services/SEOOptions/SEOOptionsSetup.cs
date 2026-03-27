using BDO;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.Extensions.Options;
using System.Linq;

namespace GWTXbyKWeb.Components.Services.SEOOptions;
public class SEOOptionsSetup : IConfigureOptions<SEOOptions>
{
    private readonly IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;

    public SEOOptionsSetup(IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
    {
        this.seoSettingsInfoProvider = seoSettingsInfoProvider;
        this.websiteChannelContext = websiteChannelContext;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
    }

    public void Configure(SEOOptions options)
    {
        int currentChannelID = websiteChannelContext.WebsiteChannelID;

        var channelSettings = webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentChannelID)
            .FirstOrDefault();

        var seoSettings = seoSettingsInfoProvider.Get()
            .WhereEquals(nameof(SEOSettingsInfo.SeoSettingsWebChannelSettingID), channelSettings?.WebChannelSettingsID ?? 0)
            .FirstOrDefault();

        options.RobotsText = seoSettings?.SeoSettingsRobots ?? string.Empty;
        options.SecurityText = seoSettings?.SeoSettingsSecurity ?? string.Empty;
    }
}