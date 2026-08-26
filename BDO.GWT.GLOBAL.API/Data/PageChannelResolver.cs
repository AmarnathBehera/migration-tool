using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;

namespace BDO.GWT.GLOBAL.API.Data
{

    public sealed class PageChannelResolver(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider)
        : IPageChannelResolver
    {
        public async Task<string?> GetChannelNameByPageGuidAsync(Guid pageGuid, CancellationToken cancellationToken = default)
        {
            var webPageItem = (await webPageItemInfoProvider.Get()
                .WhereEquals(nameof(WebPageItemInfo.WebPageItemGUID), pageGuid)
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (webPageItem is null)
            {
                return null;
            }

            var websiteChannel = (await websiteChannelInfoProvider.Get()
                .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), webPageItem.WebPageItemWebsiteChannelID)
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (websiteChannel is null)
            {
                return null;
            }

            var channel = await channelInfoProvider.GetAsync(websiteChannel.WebsiteChannelChannelID, cancellationToken);
            return channel?.ChannelName;
        }
    }
}
