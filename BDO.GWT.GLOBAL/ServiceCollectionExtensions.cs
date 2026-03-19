using BDO.GWT.Core.Services;
using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience;
using BDO.GWT.Xperience.LiveSite.Services;
using CMS.Core;
using CMS.DataEngine;
using GWTXbyKWeb.Components.Services.RobotsOptions;
using Microsoft.Extensions.DependencyInjection;
namespace BDO.GWT.Common;

public static class ServiceCollectionExtensions
{
    public static void AddXperience(this IServiceCollection serviceCollection)
    {

        serviceCollection.AddScoped<GlobalSettingsService>();
        serviceCollection.AddScoped<IWebsiteContext, WebsiteContext>();
        serviceCollection.AddScoped<IHeaderService, HeaderService>();
        serviceCollection.AddScoped<IFooterService, FooterService>();
        serviceCollection.ConfigureOptions<RobotsOptionsSetup>();
    }

}
