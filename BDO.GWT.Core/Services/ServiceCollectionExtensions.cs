namespace BDO.GWT.Core
{

    using CMS.ContentEngine;
    using CMS.Helpers;
    using CMS.Websites;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentServices(this IServiceCollection services)
        {
            // XbyK infrastructure services are typically already registered by Kentico
            // You just wire your abstraction here.
            services.AddScoped<IContentService, ContentService>();

            return services;
        }
    }
}

