using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.AspNetCore.Http;
using System.Reflection;

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
        var websiteChannelContext = ResolveWebsiteChannelContext();
        if (websiteChannelContext is null)
        {
            return null;
        }

        var websiteChannelId = GetWebsiteChannelId(websiteChannelContext);
        if (websiteChannelId <= 0)
        {
            return null;
        }

        var websiteChannel = await websiteChannelInfoProvider.GetAsync(websiteChannelId, cancellationToken);
        return websiteChannel?.WebsiteChannelDomain;
    }

    private object? ResolveWebsiteChannelContext() =>
        httpContextAccessor.HttpContext?.RequestServices.GetService(
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t is { IsInterface: true, Name: "IWebsiteChannelContext" }));

    private static int GetWebsiteChannelId(object websiteChannelContext)
    {
        var property = websiteChannelContext.GetType().GetProperty("WebsiteChannelID", BindingFlags.Instance | BindingFlags.Public);
        return property is null ? 0 : Convert.ToInt32(property.GetValue(websiteChannelContext));
    }
}
