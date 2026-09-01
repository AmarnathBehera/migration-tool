using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Sync.Options;
using BDO.GWT.GLOBAL.Sync.Services;

namespace BDO.GWT.GLOBAL.Sync;

/// <summary>
/// Extension methods for configuring API services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API-related services to the service collection, including HTTP clients, memory cache, and configuration options.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void AddApiService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<SyncAppOptions>(configuration);
        serviceCollection.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();
        serviceCollection.AddScoped<ICultureService, CultureService>();

        serviceCollection.AddScoped<IGlobalDealsSynchronizer, GlobalDealsSynchronizer>();
        serviceCollection.AddScoped<IGlobalSiteDealsService, GlobalSiteDealsService>();
        serviceCollection.AddScoped<IChannelDirectoryService, ChannelDirectoryService>();
    }
}
