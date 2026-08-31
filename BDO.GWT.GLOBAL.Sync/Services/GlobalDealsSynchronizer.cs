using BDO.GWT.GLOBAL.Sync.Options;
using Microsoft.Extensions.Options;

namespace BDO.GWT.GLOBAL.Sync.Services;

public interface IGlobalDealsSynchronizer
{
    Task SynchronizeGlobalDealsAsync(bool forceUpdate, CancellationToken cancellationToken);
}

public class GlobalDealsSynchronizer : IGlobalDealsSynchronizer
{
    private readonly IOptions<SyncAppOptions> _syncAppOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalDealsSynchronizer(IOptions<SyncAppOptions> syncAppOptions, IServiceScopeFactory serviceScopeFactory)
    {
        _syncAppOptions = syncAppOptions;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SynchronizeGlobalDealsAsync(bool forceUpdate, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var channelDirectory = scope.ServiceProvider.GetRequiredService<IChannelDirectoryService>();
        var globalSiteDealsService = scope.ServiceProvider.GetRequiredService<IGlobalSiteDealsService>();

        var globalChannel = await channelDirectory.GetChannelAsync(_syncAppOptions.Value.GlobalSiteGuid, cancellationToken);
        if (globalChannel is null)
        {
            return;
        }

        var sourceChannels = await channelDirectory.GetAllChannelsExceptAsync(_syncAppOptions.Value.GlobalSiteGuid, cancellationToken);
        if (sourceChannels.Count == 0)
        {
            return;
        }

        await globalSiteDealsService.SynchronizeChannelGlobalDealsAsync(globalChannel, sourceChannels, forceUpdate, cancellationToken);
    }
}