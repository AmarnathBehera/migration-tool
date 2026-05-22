namespace BDO.GWT.Core
{
    using BDO.GWT.Core;
    using BDO.GWT.Core.services;
    using BDO.GWT.Core.Services;
    using BDO.GWT.Core.Services.Header;
    using BDO.GWT.Core.ValueObjects;
    using CMS.ContentEngine;
    using CMS.Helpers;
    using CMS.Websites;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions

    {
        public static IServiceCollection AddContentServices(this IServiceCollection services)
        {
            // XbyK infrastructure services are typically already registered by Kentico
            // You just wire your abstraction here.
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<IWebPageUrlResolver, WebPageUrlResolver>();
            services.AddScoped<IPageUrlService, PageUrlService>();
            services.AddScoped<IPageUtility, PageUtility>();
            services.AddScoped<IHomePageService, HomePageService>();
            services.AddScoped<IHeaderService, HeaderService>();
            services.AddScoped<IWebsiteCulturesService, WebsiteCulturesService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICultureContext, CultureContext>();
            //services.AddScoped<IPageMetadataService, PageMetadataService>();

            return services;
        }
    }
}

