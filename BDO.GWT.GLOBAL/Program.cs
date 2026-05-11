using BDO;
using BDO.GWT.Common;
using Kentico.Membership;
using Kentico.Web.Mvc;
using BDO.GWT.Common.Options;
using BDO.GWT.Core;
using BDO.GWT.Domain.Services;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Kentico.Activities.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using XperienceCommunity.MultiChoiceFormControl.Admin.Helpers.Components.MultiChoice;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions
    {
        DefaultSectionIdentifier = ComponentIdentifiers.Sections.Container,
        RegisterDefaultSection = true,
        ContentTypeNames = new[]
        {
          HomePage.CONTENT_TYPE_NAME,
          SectionAbout.CONTENT_TYPE_NAME,
          AboutPage.CONTENT_TYPE_NAME,
          Contact.CONTENT_TYPE_NAME,
          Person.CONTENT_TYPE_NAME,
          Locations.CONTENT_TYPE_NAME,
          LocationCity.CONTENT_TYPE_NAME,
          SectionIndustries.CONTENT_TYPE_NAME,
          IndustryCategory.CONTENT_TYPE_NAME,
          Industry.CONTENT_TYPE_NAME,
          IndustryService.CONTENT_TYPE_NAME,
          Sub_Service.CONTENT_TYPE_NAME
       }
    }); features.UseActivityTracking();
    features.UseWebPageRouting();
    features.UseEmailStatisticsLogging();
    features.UseEmailMarketing();
});

builder.Configuration.AddInMemoryCollection(
    builder.Configuration
        .GetSection("AzureStorageProviderSettings")
        .GetChildren().ToDictionary(x => x.Key, x => x.Value)
);
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});
builder.Services.AddTransient<IMultiChoiceOptionsProviderActivator, MultiChoiceOptionsProviderActivator>();

builder.Services.Configure<ReactOptions>(
    builder.Configuration.GetSection("ReactOptions"));
builder.Services.Configure<ReactCachingOptions>(
    builder.Configuration.GetSection("ReactCachingOptions"));

builder.Services.AddXperience();

builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

builder.Services.AddAuthentication();
builder.Services.AddContentServices();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPageBuilderService, PageBuilderService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddScoped<ISiteContext, SiteContext>();
builder.Services.AddScoped<ISiteConfigurationService, SiteConfigurationService>();
builder.Services.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddXperience();
builder.Services.AddControllersWithViews();
builder.Services.AddXperienceCommunityLocalization();
builder.Services.AddAdminExternalAuthenticationProvider(
    authBuilder => authBuilder.AddMicrosoftIdentityWebApp(options =>
    {

        options.Domain = "";

        options.TenantId = builder.Configuration["AzureAd:TenantId"];

        options.ClientId = builder.Configuration["AzureAd:ClientId"];

        // Add the credentials used to prove your application's identity,
        options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];

        options.Instance = "https://login.microsoftonline.com/";

        options.CallbackPath = new PathString("/signin-oidc");

        options.ResponseType = OpenIdConnectResponseType.Code;
    }),
    options =>
    {
        // Sets cookies as the sign in scheme
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Sets OIDC as the authentication method
        options.AuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
        // Xperience synchronizes the user account with data from the external provider every time a user signs in
        options.UserSynchronizationFrequency = UserSynchronizationFrequency.Always;
    }
);

builder.Services.Configure<AdminIdentityOptions>(options =>

{

    options.AuthenticationOptions.Mode = AdminAuthenticationMode.MaintainForms;

});

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseKentico();

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
