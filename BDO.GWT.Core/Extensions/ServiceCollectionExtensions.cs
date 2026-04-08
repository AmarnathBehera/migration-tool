using BDO.GWT.Common.Options;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddWeb(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<CachingOptions>()
            .BindConfiguration("Caching")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactCachingOptions>()
            .BindConfiguration("Caching:React")
            .ValidateDataAnnotations();
    }
    public static void AddLiveSite(this IServiceCollection services)
    {
        services.AddOptions<ReactOptions>()
            .BindConfiguration("React")
            .ValidateDataAnnotations();

        services.AddHttpClient();

        //services.AddScoped<IAboutPageService<AboutPage>, AboutPageService<AboutPage>>();
        //services.AddScoped<IBreadcrumbsService, BreadcrumbsService>();
        //services.AddScoped<ICanonicalLinkService, CanonicalLinkService>();
        //services.AddScoped<ICookiebotService, CookiebotService>();
        //services.AddScoped<ICurrentWebsiteHelper, CurrentWebsiteHelper>();
        //services.AddScoped<IFooterService, FooterService>();
        //services.AddScoped<IGoogleServicesService, GoogleServicesService>();
        //services.AddScoped<IHeaderService, HeaderService>();
        //services.AddScoped<IHeroService, HeroService>();
        //services.AddScoped<ILiveSiteContext, LiveSiteContext>();
        //services.AddScoped<IOGMetadataService, OGMetadataService>();
        services.AddScoped<IReactService, ReactService>();
        //services.AddScoped<IRobotsMetaService, RobotsMetaService>();
        //services.AddScoped<IScriptsService, ScriptsService>();
        //services.AddScoped<IScriptsWhenCookiesAcceptedService, ScriptsWhenCookiesAcceptedService>();
    }

    public static void AddXperience(this IServiceCollection serviceCollection)
    {
        //foreach (var pageTypeMap in TypeMapper.PageTypeMaps)
        //{
        //    pageTypeMap.AddServices(serviceCollection);
        //}

        //foreach (var entityTypeMap in TypeMapper.EntityTypeMaps)
        //{
        //    entityTypeMap.AddServices(serviceCollection);
        //}

        //serviceCollection.AddSingleton<ICachingService, CachingService>();
        //serviceCollection.AddScoped<ICultureContext, CultureContext>();
        //serviceCollection.AddSingleton<IEntitiesCacheDependenciesHelper, EntitiesCacheDependenciesHelper>();
        //serviceCollection.AddSingleton<IMapper>(_ => new Mapper(AutoMapperConfiguration.MapperConfiguration));
        //serviceCollection.AddScoped<IMultiTypePageRetriever, MultiTypePageRetriever>();
        //serviceCollection.AddSingleton<IPagesCacheDependenciesHelper, PagesCacheDependenciesHelper>();
        //serviceCollection.AddScoped<IPreviewContext, PreviewContext>();
        //serviceCollection.AddSingleton<IQueryMapper, QueryMapper>();
        serviceCollection.AddSingleton<IReactCachingService, ReactCachingService>();
        //serviceCollection.AddSingleton<ITypeMapper, TypeMapper>();
        //serviceCollection.AddScoped<IVisitorContext, VisitorContext>();
        //serviceCollection.AddScoped<IWebsiteContext, WebsiteContext>();
    }
}



