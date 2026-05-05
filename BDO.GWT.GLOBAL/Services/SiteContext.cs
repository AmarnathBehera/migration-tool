using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.GLOBAL.Services;

public class SiteContext : ISiteContext
{
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelProvider;

    public SiteContext(
        IWebsiteChannelContext websiteChannelContext,
        IInfoProvider<WebsiteChannelInfo> websiteChannelProvider)
    {
        _websiteChannelContext = websiteChannelContext;
        _websiteChannelProvider = websiteChannelProvider;
    }

    public int CurrentSiteID => _websiteChannelContext.WebsiteChannelID;

    public string CurrentSiteName => _websiteChannelContext.WebsiteChannelName;
}
