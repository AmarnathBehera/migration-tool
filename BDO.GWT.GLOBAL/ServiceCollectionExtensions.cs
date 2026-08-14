using BDO.GWT.Common.Options;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using BDO.GWT.Xperience.Core.Services;
using CMS.Core;
using GWTXbyKWeb.Components.Services.SEOOptions;
using Microsoft.Extensions.DependencyInjection;


namespace BDO.GWT.Common;

public static class ServiceCollectionExtensions
{
    public static void AddXperience(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<CachingOptions>().BindConfiguration("Caching").ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactCachingOptions>() .BindConfiguration("Caching:React").ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactOptions>()
          .BindConfiguration("React")
          .ValidateDataAnnotations();

        serviceCollection.AddHttpClient();
        serviceCollection.AddScoped<IPageContext, XperiencePageContext>();
        serviceCollection.AddScoped<XperiencePageContext>();
        serviceCollection.AddScoped<IWebsiteContext, WebsiteContext>();
        serviceCollection.ConfigureOptions<SEOOptionsSetup>();
        serviceCollection.AddScoped<IPageBuilderSectionService, PageBuilderSectionService>();
        serviceCollection.AddTransient<DropdownScriptExecutor>();

        serviceCollection.AddScoped<IChannelAccessService, ChannelAccessService>();
        serviceCollection.AddScoped<IMicrositeMasterPageService, MicrositeMasterPageService>();
        serviceCollection.AddScoped<IMicrositePageService, MicrositePageService>();
        serviceCollection.AddScoped<IBreadcrumbsService, BreadcrumbsService>();
        serviceCollection.AddScoped<IMicrositeHeaderService, MicrositeHeaderService>();

        serviceCollection.AddScoped<IGDPRUserLookupService, GDPRUserService>();
        serviceCollection.AddScoped<IApiPermissionService, ApiPermissionService>();
    }

}
