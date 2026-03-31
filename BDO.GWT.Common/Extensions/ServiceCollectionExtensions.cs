//using BDO.GWT.Domain.Entities.Pages;
//using BDO.GWT.Web.LiveSite.Helpers;
//using BDO.GWT.Web.LiveSite.Options;
//using BDO.GWT.Web.LiveSite.Services;
//using Microsoft.Extensions.DependencyInjection;

//namespace BDO.GWT.Web.LiveSite.Extensions;

//public static class ServiceCollectionExtensions
//{
//    public static void AddLiveSite(this IServiceCollection services)
//    {
//        services.AddOptions<ReactOptions>()
//            .BindConfiguration("React")
//            .ValidateDataAnnotations();

//        services.AddHttpClient();

//        services.AddScoped<IAboutPageService<AboutPage>, AboutPageService<AboutPage>>();
//        services.AddScoped<IBreadcrumbsService, BreadcrumbsService>();
//        services.AddScoped<ICanonicalLinkService, CanonicalLinkService>();
//        services.AddScoped<ICookiebotService, CookiebotService>();
//        services.AddScoped<ICurrentWebsiteHelper, CurrentWebsiteHelper>();
//        services.AddScoped<IFooterService, FooterService>();
//        services.AddScoped<IGoogleServicesService, GoogleServicesService>();
//        services.AddScoped<IHeaderService, HeaderService>();
//        services.AddScoped<IHeroService, HeroService>();
//        services.AddScoped<ILiveSiteContext, LiveSiteContext>();
//        services.AddScoped<IOGMetadataService, OGMetadataService>();
//        services.AddScoped<IReactService, ReactService>();
//        services.AddScoped<IRobotsMetaService, RobotsMetaService>();
//        services.AddScoped<IScriptsService, ScriptsService>();
//        services.AddScoped<IScriptsWhenCookiesAcceptedService, ScriptsWhenCookiesAcceptedService>();
//    }
//}
