using CMS.Websites.Routing;

namespace BDO.GWT.Xperience.Core.Services;

public interface ISiteContext
{
    int CurrentSiteID { get; }

    string CurrentSiteName { get; }
}

public class SiteContext : ISiteContext
{
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public SiteContext(IWebsiteChannelContext websiteChannelContext)
    {
        _websiteChannelContext = websiteChannelContext;
    }

    public int CurrentSiteID => _websiteChannelContext.WebsiteChannelID;

    public string CurrentSiteName => _websiteChannelContext.WebsiteChannelName;
}
