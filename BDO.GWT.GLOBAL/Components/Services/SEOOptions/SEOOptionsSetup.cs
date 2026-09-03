using BDO;
using CMS.DataEngine;
using CMS.Websites.Routing;
using Microsoft.Extensions.Options;
using System.Linq;

namespace GWTXbyKWeb.Components.Services.SEOOptions;
public class SEOOptionsSetup : IConfigureOptions<SEOOptions>
{
    private readonly IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider;
    private readonly IWebsiteChannelContext websiteChannelContext;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;

    public SEOOptionsSetup(IInfoProvider<GlobalsettingskeyInfo> provider,IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider, IWebsiteChannelContext websiteChannelContext, IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider)
    {
        this.webChannelSnippetInfoProvider = webChannelSnippetInfoProvider;
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

        var seoSettings = webChannelSnippetInfoProvider.Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), channelSettings?.WebChannelSettingsID ?? 0)
            .FirstOrDefault();
        if(seoSettings.SeoSettingsRobotsInheritfromglobal)
        {
            var globalSettings = _provider.Get().FirstOrDefault();
            options.RobotsText = globalSettings?.SeoSettingsRobots ?? string.Empty;
        }
        else
        {
            options.RobotsText = seoSettings?.SeoSettingsRobots ?? string.Empty;
        }
        if(seoSettings.SeoSettingsSecurityInheritfromGlobal)
        {
            var globalSettings = _provider.Get().FirstOrDefault();
            options.SecurityText = globalSettings?.SeoSettingsRobots ?? string.Empty;
        }
        else
        {
            options.SecurityText = seoSettings?.SeoSettingsSecurity ?? string.Empty;
        }
       
    }
}