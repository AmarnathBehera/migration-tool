using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.GLOBAL.API.ApiClients;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Middleware;
using BDO.GWT.GLOBAL.API.Options;
using BDO.GWT.GLOBAL.API.Services;
using CMS.MacroEngine;

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
        serviceCollection.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();
        serviceCollection.AddScoped<MemberFirmMiddleware>();

        serviceCollection.AddOptions<MemberFirmServiceOptions>()
            .BindConfiguration("MemberFirmService")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<MasterDataApiOptions>()
            .BindConfiguration("MasterDataApi")
            .ValidateDataAnnotations();

        serviceCollection.AddScoped<IMemberFirmService, MemberFirmService>();
        serviceCollection.AddScoped<IMasterDataApiClient, MasterDataApiClient>();
        serviceCollection.AddScoped<IFormCrudService, FormCrudService>();
        serviceCollection.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
        serviceCollection.AddScoped<IContentService, ContentService>();
        serviceCollection.AddScoped<IValidationState, ValidationState>();
        serviceCollection.AddScoped<ICultureService, CultureService>();
        serviceCollection.AddScoped<ICultureContext, CultureContext>();
        serviceCollection.AddScoped<IWebsiteCulturesService, WebsiteCulturesService>();
        serviceCollection.AddScoped<IPageContentValidator, PageContentValidator>();
        serviceCollection.AddScoped<IMacroResolver, MacroResolver>();
        serviceCollection.AddScoped<IGlobalLocationCustomTableDataService, GlobalLocationCustomTableDataService>();
        serviceCollection.AddScoped<IDealCrudService, DealCrudService>();
        serviceCollection.AddScoped<IFooterService, FooterService>();
        serviceCollection.AddScoped<ICultureValidationService, CultureValidationService>();
    }
}
