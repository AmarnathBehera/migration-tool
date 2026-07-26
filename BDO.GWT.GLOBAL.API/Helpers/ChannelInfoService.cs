using CMS.ContentEngine;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Helpers
{
    public interface IChannelInfoService
    {
        Task<string?> GetChannelNameAsync(int channelId, CancellationToken cancellationToken);
    }

    public sealed class ChannelInfoService(IInfoProvider<ChannelInfo> channelInfoProvider)
        : IChannelInfoService
    {
        public async Task<string?> GetChannelNameAsync(int channelId, CancellationToken cancellationToken)
        {
            var channel = await channelInfoProvider.GetAsync(channelId, cancellationToken);
            return channel?.ChannelName;
        }
    }
}