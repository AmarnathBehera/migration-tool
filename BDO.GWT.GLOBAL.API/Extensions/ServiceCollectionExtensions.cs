using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.API.ApiClients;
using BDO.GWT.GLOBAL.API.Data;
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
        serviceCollection.AddScoped<IValidationState, ValidationState>();
        serviceCollection.AddScoped<IAuthorizationHelper, AuthorizationHelper>();

        serviceCollection.AddScoped<IPageContentValidator, PageContentValidator>();
        serviceCollection.AddScoped<IMacroResolver, MacroResolver>();
        serviceCollection.AddScoped<IMappingHelper, MappingHelper>();
        serviceCollection.AddScoped<ICultureService, CultureService>();
        serviceCollection.AddScoped<ICultureValidationService, CultureValidationService>();

        serviceCollection.AddScoped<IFormCrudService, FormCrudService>();
        serviceCollection.AddScoped<IDealCrudService, DealCrudService>();
        serviceCollection.AddScoped<ICareerCrudService, CareerCrudService>();
        serviceCollection.AddScoped<IPersonCrudService, PersonCrudService>();
        serviceCollection.AddScoped<IInsightCrudService, InsightCrudService>();
        serviceCollection.AddScoped<ISectionEventsCrudService, SectionEventsCrudService>();
        serviceCollection.AddScoped<ILocationOfficeCrudService, LocationOfficeCrudService>();
        serviceCollection.AddScoped<IGlobalSettingCrudService, GlobalSettingCrudService>();
        serviceCollection.AddScoped<IChannelSettingCrudService, ChannelSettingCrudService>();
        serviceCollection.AddScoped<IChannelCrudService, ChannelCrudService>();
        serviceCollection.AddScoped<IChannelInfoService, ChannelInfoService>();
        serviceCollection.AddScoped<IChannelWebsiteDomainService, ChannelWebsiteDomainService>();
        serviceCollection.AddScoped<IUserCrudService, UserCrudService>();
        serviceCollection.AddScoped<IUserApiPermissionsCrudService, UserApiPermissionsCrudService>();
        serviceCollection.AddScoped<ILicenseKeysCrudService, LicenseKeysCrudService>();
        serviceCollection.AddScoped<IScheduledTaskCrudService, ScheduledTaskCrudService>();
        serviceCollection.AddScoped<IPageAttachmentService, PageAttachmentService>();
        serviceCollection.AddScoped<IPageChannelResolver, PageChannelResolver>();
        serviceCollection.AddScoped<IAssetUrlResolver, AssetUrlResolver>();

        serviceCollection.AddScoped<IContentItemInfoService, ContentItemInfoService>();
        serviceCollection.AddScoped<IPageAttachmentCrudService, PageAttachmentCrudService>();
        serviceCollection.AddScoped<IContentQueryMapper, ContentQueryMapper>();
        serviceCollection.AddScoped<IMediaFileCrudService, MediaFileCrudService>();
        serviceCollection.AddScoped<IContentCategoryService, ContentCategoryService>();
        serviceCollection.AddScoped<IEventLogCrudService, EventLogCrudService>();
        serviceCollection.AddScoped<ISettingsCrudHelper, SettingsCrudHelper>();
        serviceCollection.AddScoped<ICustomGlobalLocationsService, CustomGlobalLocationsService>();
        serviceCollection.AddScoped<IRoleCrudService, RoleCrudService>();
    }
}
