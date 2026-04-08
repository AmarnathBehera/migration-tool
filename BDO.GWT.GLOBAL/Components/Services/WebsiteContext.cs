using BDO.GWT.Domain.Services;
using CMS.Websites.Routing;

namespace BDO.GWT.Common;

public class WebsiteContext : IWebsiteContext
{
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public WebsiteContext(IWebsiteChannelContext websiteChannelContext)
    {
        _websiteChannelContext = websiteChannelContext;
    }

    public int CurrentWebsiteID => _websiteChannelContext.WebsiteChannelID;

    public string CurrentWebsiteName => _websiteChannelContext.WebsiteChannelName;

    public bool IsPreview => _websiteChannelContext.IsPreview;
}