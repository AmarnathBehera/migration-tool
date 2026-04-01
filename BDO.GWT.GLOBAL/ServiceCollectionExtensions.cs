using BDO.GWT.Common.Options;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Services;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Xperience;
using GWTXbyKWeb.Components.Services.SEOOptions;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.Common;

public static class ServiceCollectionExtensions
{
    public static void AddXperience(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<CachingOptions>()
           .BindConfiguration("Caching")
           .ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactCachingOptions>()
            .BindConfiguration("Caching:React")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactOptions>()
          .BindConfiguration("React")
          .ValidateDataAnnotations();

        serviceCollection.AddHttpClient();

        serviceCollection.AddScoped<GlobalSettingsService>();
        serviceCollection.AddScoped<IWebsiteContext, WebsiteContext>();
        serviceCollection.AddScoped<IHeaderService, HeaderService>();
        serviceCollection.AddScoped<IFooterService, FooterService>();
        serviceCollection.ConfigureOptions<SEOOptionsSetup>();
        serviceCollection.AddScoped<IReactService, ReactService>();
        serviceCollection.AddSingleton<IReactCachingService, ReactCachingService>();
         serviceCollection.AddScoped<IPageContext, XperiencePageContext>();
        serviceCollection.AddScoped<ITileContentService, TileContentService>();

    }

}
