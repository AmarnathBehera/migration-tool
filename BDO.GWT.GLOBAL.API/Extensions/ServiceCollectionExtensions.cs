using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.API.ApiClients;
using BDO.GWT.Web.Core.API.Helpers;
using BDO.GWT.Web.Core.API.Middleware;
using BDO.GWT.Web.Core.API.Options;
using BDO.GWT.Web.Core.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.Web.Core.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApi(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();

        serviceCollection.AddScoped<MemberFirmMiddleware>();

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
        serviceCollection.AddScoped<IMasterDataApiClient, MasterDataApiClient>();
        //serviceCollection.AddScoped<IMediaFileCrudService, MediaFileCrudService>();
        serviceCollection.AddScoped<IMemberFirmService, MemberFirmService>();
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
