using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.GLOBAL.API.Helpers;

public interface IChannelWebsiteDomainService
{
    Task<string?> GetWebsiteDomainAsync(string channelName, CancellationToken cancellationToken = default);
    Task<string?> GetCurrentWebsiteDomainAsync(CancellationToken cancellationToken = default);
}

public sealed class ChannelWebsiteDomainService(
    IInfoProvider<ChannelInfo> channelInfoProvider,
    IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
    IHttpContextAccessor httpContextAccessor)
    : IChannelWebsiteDomainService
{
    public async Task<string?> GetWebsiteDomainAsync(string channelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(channelName))
        {
            return null;
        }

        var channel = (await channelInfoProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelDomain))
            .WhereEquals(nameof(ChannelInfo.ChannelName), channelName)
            .TopN(1)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return channel?.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))?.ToString();
    }

    public async Task<string?> GetCurrentWebsiteDomainAsync(CancellationToken cancellationToken = default)
    {
        var websiteChannelContext = httpContextAccessor.HttpContext?.RequestServices.GetService<IWebsiteChannelContext>();
        if (websiteChannelContext is null)
        {
            return null;
        }

        var websiteChannelId = websiteChannelContext.WebsiteChannelID;
        if (websiteChannelId <= 0)
        {
            return null;
        }

        var websiteChannel = (await websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), websiteChannelId)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return websiteChannel?.WebsiteChannelDomain;
    }
}
