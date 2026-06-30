using BDO.GWT.GLOBAL.API.ApiClients;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Middleware;
using BDO.GWT.GLOBAL.API.Options;
using BDO.GWT.GLOBAL.API.Services;

namespace BDO.GWT.GLOBAL.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApi(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();

        //serviceCollection.AddScoped<MemberFirmMiddleware>();

        serviceCollection.AddOptions<MemberFirmServiceOptions>()
            .BindConfiguration("MemberFirmService")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<MasterDataApiOptions>()
            .BindConfiguration("MasterDataApi")
            .ValidateDataAnnotations();

        //serviceCollection.AddOptions<RegionOptions>()
        //    .BindConfiguration("Region")
        //    .ValidateDataAnnotations();

        //serviceCollection.AddScoped<IAdvancedSettingsService, AdvancedSettingsService>();
        //serviceCollection.AddScoped<IAttachmentHistoryService, AttachmentHistoryService>();
        serviceCollection.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
        //serviceCollection.AddScoped<ICareerCrudService, CareerCrudService>();
        //serviceCollection.AddScoped<IContentService, ContentService>();
        //serviceCollection.AddScoped<ICultureHelper, CultureHelper>();
        //serviceCollection.AddScoped<IDealCrudService, DealCrudService>();
        //serviceCollection.AddScoped<IEntityHelper<Site>, EntityHelper<Site>>();
        //serviceCollection.AddScoped<IEventLogCrudService, EventLogCrudService>();
        //serviceCollection.AddScoped<IFormCrudService, FormCrudService>();
        //serviceCollection.AddScoped<IGlobalSettingCrudService, GlobalSettingCrudService>();
        //serviceCollection.AddScoped<IInsightCrudService, InsightCrudService>();
        //serviceCollection.AddScoped<ILicenseKeysCrudService, LicenseKeysCrudService>();
        //serviceCollection.AddScoped<ILocationOfficeCrudService, LocationOfficeCrudService>();
        //serviceCollection.AddScoped<IMasterDataApiClient, MasterDataApiClient>();
        //serviceCollection.AddScoped<IMediaFileCrudService, MediaFileCrudService>();
        //serviceCollection.AddScoped<IMemberFirmService, MemberFirmService>();
        //serviceCollection.AddScoped<IPageAttachmentCrudService, PageAttachmentCrudService>();
        //serviceCollection.AddScoped<IPersonCrudService, PersonCrudService>();
        //serviceCollection.AddScoped<IScheduledTaskCrudService, ScheduledTaskCrudService>();
        //serviceCollection.AddScoped<ISectionEventsCrudService, SectionEventsCrudService>();
        //serviceCollection.AddScoped<ISettingsCrudHelper, SettingsCrudHelper>();
        //serviceCollection.AddScoped<ISiteCrudService, SiteCrudService>();
        //serviceCollection.AddScoped<ISiteDomainAliasHelper, SiteDomainAliasHelper>();
        //serviceCollection.AddScoped<ISiteSettingCrudService, SiteSettingCrudService>();
        //serviceCollection.AddScoped<IUserApiPermissionsCrudService, UserApiPermissionsCrudService>();
        //serviceCollection.AddScoped<IUserCrudService, UserCrudService>();
    }
}
