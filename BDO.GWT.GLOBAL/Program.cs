using BDO;
using BDO.GWT.Common;
using BDO.GWT.Common.Options;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Kentico.Activities.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using XperienceCommunity.MultiChoiceFormControl.Admin.Helpers.Components.MultiChoice;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions
    {
        DefaultSectionIdentifier = ComponentIdentifiers.Sections.Container,
        RegisterDefaultSection = true,
        ContentTypeNames = new[]
        {
          "BDO.HomePage",
          "BDO.SectionAbout",
          "BDO.AboutPage",
          "BDO.HomePage",
          "BDO.Contact",
          Person.CONTENT_TYPE_NAME,
          Locations.CONTENT_TYPE_NAME,
          LocationCity.CONTENT_TYPE_NAME
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
builder.Services.AddScoped<IErrorService, ErrorService>();
builder.Services.AddScoped<IEventLogging, EventLogging>();
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

var app = builder.Build();

app.InitKentico();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseKentico();

app.Kentico().MapRoutes();

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
