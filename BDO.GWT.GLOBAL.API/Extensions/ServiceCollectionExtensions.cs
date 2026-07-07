using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Options;

namespace BDO.GWT.GLOBAL.API.Extensions;

/// <summary>
/// Extension methods for configuring API services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API-related services to the service collection, including HTTP clients, memory cache, and configuration options.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add services to.</param>
    public static void AddApi(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();

        serviceCollection.AddOptions<MemberFirmServiceOptions>()
            .BindConfiguration("MemberFirmService")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<MasterDataApiOptions>()
            .BindConfiguration("MasterDataApi")
            .ValidateDataAnnotations();

        
        serviceCollection.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
    }
}
