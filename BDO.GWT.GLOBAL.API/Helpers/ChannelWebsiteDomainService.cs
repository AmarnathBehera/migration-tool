using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;

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
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        // Get current request domain
        var currentDomain = GetCurrentRequestDomain(httpContext);

        var websiteChannelContext = httpContext.RequestServices.GetService<IWebsiteChannelContext>();
        if (websiteChannelContext is null || websiteChannelContext.WebsiteChannelID <= 0)
        {
            // No valid channel context, try to lookup channel by current domain
            return await GetDomainByCurrentHostAsync(httpContext, cancellationToken) ?? currentDomain;
        }

        // Channel context is available and valid
        var websiteChannelId = websiteChannelContext.WebsiteChannelID;
        var websiteChannel = (await websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), websiteChannelId)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return websiteChannel?.WebsiteChannelDomain ?? currentDomain;
    }

    private async Task<string?> GetDomainByCurrentHostAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var currentHost = httpContext.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(currentHost))
        {
            return null;
        }

        var websiteChannel = (await websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelDomain), currentHost)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (websiteChannel?.WebsiteChannelDomain is not null)
        {
            return websiteChannel.WebsiteChannelDomain;
        }

        if (currentHost.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            var firstChannel = (await websiteChannelInfoProvider.Get()
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault();

            return firstChannel?.WebsiteChannelDomain;
        }

        return null;
    }

    private static string? GetCurrentRequestDomain(HttpContext httpContext)
    {
        var host = httpContext.Request.Host;
        var scheme = httpContext.Request.Scheme;

        if (host.HasValue)
        {
            return $"{scheme}://{host}";
        }

        return null;
    }
}
