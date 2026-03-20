using BDO;
using BDO.GWT.Common;
using Kentico.Membership;
using Kentico.Web.Mvc;
using BDO.GWT.Core;
using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Kentico.Activities.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;

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

builder.Services.AddAdminExternalAuthenticationProvider(
    builder => builder.AddMicrosoftIdentityWebApp(options =>
    {

        options.Domain = "";

        options.TenantId = "44f4e7a6-4821-44d7-b286-cd90436c6975";

        options.ClientId = "422caa30-f1da-461f-99d5-39f2413edf07";

        options.Instance = "https://login.microsoftonline.com/";

        options.CallbackPath = new PathString("/signin-oidc");

        options.ResponseType = OpenIdConnectResponseType.Code;


        // Add the credentials used to prove your application's identity,
        // either a certificate OR a client secret.
        options.ClientSecret = "jjm8Q~y-tfGYE5xMRE3JKKdTibZlkWsv_c-GVco-";

        ////// Certificate
        //options.ClientCertificates = new List<CertificateDescription>
        //{
        //    // Replace the examples of certificate descriptions with your own

        //    // Certificate from Azure Key Vault
        //    CertificateDescription.FromKeyVault("https://keyvault-name.vault.azure.net/", "certificate-name"),
        //    // Certificate from a local file
        //   CertificateDescription.FromPath("path_to_certificate.pfx","certificate_password")
        //};
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
