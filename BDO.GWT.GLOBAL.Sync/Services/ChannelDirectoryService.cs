using BDO.GWT.GLOBAL.Sync.Data;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Websites;

namespace BDO.GWT.GLOBAL.Sync.Services;

public interface IChannelDirectoryService
{
    Task<ChannelOutput?> GetChannelAsync(Guid channelGuid, CancellationToken cancellationToken);

    /// <summary>
    /// Returns every website channel except the one matching <paramref name="excludeGuid"/>
    /// (typically the global site).
    /// </summary>
    Task<List<ChannelOutput>> GetAllChannelsExceptAsync(Guid excludeGuid, CancellationToken cancellationToken);
}

public class ChannelDirectoryService : IChannelDirectoryService
{
    private readonly IInfoProvider<ChannelInfo> _channelProvider;

    public ChannelDirectoryService(IInfoProvider<ChannelInfo> channelProvider)
    {
        _channelProvider = channelProvider;
    }

    public async Task<ChannelOutput?> GetChannelAsync(Guid channelGuid, CancellationToken cancellationToken)
    {
        var channels = await ChannelQuery()
            .WhereEquals(nameof(ChannelInfo.ChannelGUID), channelGuid)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var result = channels.FirstOrDefault();
        return result is null ? null : ToChannelOutput(result);
    }

    public async Task<List<ChannelOutput>> GetAllChannelsExceptAsync(Guid excludeGuid, CancellationToken cancellationToken)
    {
        var channels = await ChannelQuery()
            .WhereNotEquals(nameof(ChannelInfo.ChannelGUID), excludeGuid)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var results = new List<ChannelOutput>();
        foreach (var channel in channels)
        {
            results.Add(ToChannelOutput(channel));
        }

        return results;
    }
    private ObjectQuery<ChannelInfo> ChannelQuery() =>
        _channelProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID),
                JoinTypeEnum.Inner))
            .WhereNotNull(nameof(WebsiteChannelInfo.WebsiteChannelID))
            .Columns(
                nameof(ChannelInfo.ChannelID),
                nameof(ChannelInfo.ChannelGUID),
                nameof(ChannelInfo.ChannelDisplayName),
                nameof(ChannelInfo.ChannelName),
                nameof(WebsiteChannelInfo.WebsiteChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelDomain),
                nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID));

    private static ChannelOutput ToChannelOutput(IDataContainer channel)
    {
        string domain = channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))?.ToString() ?? string.Empty;

        var general = new SiteGeneralOutput
        {
            ID = ValidationHelper.GetInteger(channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelID)), 0),
            GUID = ValidationHelper.GetGuid(channel.GetValue(nameof(ChannelInfo.ChannelGUID)), Guid.Empty),
            DisplayName = channel.GetValue(nameof(ChannelInfo.ChannelDisplayName))?.ToString() ?? string.Empty,
            CodeName = channel.GetValue(nameof(ChannelInfo.ChannelName))?.ToString() ?? string.Empty,
            WebsitedomainresentationURL = domain,
        };

        return new ChannelOutput { General = general };
    }
}