using BDO;
using BDO.GWT.Common;
using BDO.GWT.Core;
using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Kentico.Activities.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions
    {
        DefaultSectionIdentifier = ComponentIdentifiers.Simple,
        RegisterDefaultSection = true,
        ContentTypeNames = new[]
        {
          "BDO.HomePage"
       }
    }); features.UseActivityTracking();
    features.UseWebPageRouting();
    features.UseEmailStatisticsLogging();
    features.UseEmailMarketing();
});


builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

builder.Services.AddAuthentication();
builder.Services.AddContentServices();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPageBuilderService, PageBuilderService>();

builder.Services.AddXperience();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseAuthentication();
// app.UseAuthentication();

app.UseKentico();

// app.UseAuthorization();

app.Kentico().MapRoutes();

app.MapControllerRoute(
   name: "error",
   pattern: "error/{code}",
   defaults: new { controller = "HttpErrors", action = "Error" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=About}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: $"{{{WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY}}}/{{controller}}/{{action}}",
    constraints: new
    {
        controller = "Home"
    }
);
app.Run();
