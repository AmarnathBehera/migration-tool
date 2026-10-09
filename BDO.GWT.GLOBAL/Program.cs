using AngleSharp;
using AutoMapper;
using BDO;
using BDO.GWT.Common;
using BDO.GWT.Common.Options;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Mappers;
using BDO.GWT.Core.Services;
using BDO.GWT.Domain.Services;
using BDO.GWT.GLOBAL.Search;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience.Core.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Kentico.Activities.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Kentico.Xperience.SendGrid;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SendGrid;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using XperienceCommunity.MultiChoiceFormControl.Admin.Helpers.Components.MultiChoice;
using XperienceCommunity.Redirects;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions
    {
        DefaultSectionIdentifier = ComponentIdentifiers.Sections.Container,
        RegisterDefaultSection = true,
        ContentTypeNames = PageBuilderContentTypes.ContentTypes
    }); features.UseActivityTracking();
    features.UseWebPageRouting();
    features.UseEmailStatisticsLogging();
    features.UseEmailMarketing();
});

builder.Configuration.AddInMemoryCollection(
    builder.Configuration
        .GetSection("AzureStorageProviderSettings")
        .GetChildren()
        .Where(x => x.Value != null)
        .ToDictionary(x => x.Key, x => x.Value!)
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
builder.Services.AddScoped<IDealCardsService, DealCardsService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<INonAsciiUrlReplacementCache, NonAsciiUrlReplacementCache>();
builder.Services.AddScoped<INonAsciiUrlReplacementSettingProvider, NonAsciiUrlReplacementSettingProvider>();
builder.Services.AddScoped<INonAsciiUrlReplacementService, NonAsciiUrlReplacementService>();
builder.Services.AddScoped<IPageBuilderService, PageBuilderService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddScoped<ISiteContext, SiteContext>();
builder.Services.AddScoped<SiteConfigurationProviders>();
builder.Services.AddScoped<ISiteConfigurationService, SiteConfigurationService>();
builder.Services.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Register Kentico Azure AI Search with our custom indexing strategy.
// The strategy name ("GlobalSearchStrategy") is selected per-index in the Xperience admin UI.
builder.Services.AddKenticoAzureSearch(searchBuilder =>
{
    searchBuilder.RegisterStrategy<GlobalAzureSearchIndexingStrategy, GlobalSearchModel>("GlobalSearchStrategy");
}, builder.Configuration);

// Register AutoMapper so IMapper can be resolved for SearchService / SearchQueryHelper.
// Scan the assemblies that contain your AutoMapper Profile classes and the search types.
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(
        typeof(BDO.GWT.Core.Helpers.SearchQueryHelper).Assembly,        // BDO.GWT.Core
        typeof(BDO.GWT.AzureCognitiveSearch.Services.SearchService).Assembly);
    cfg.AddProfile<SearchDocumentProfile>();
});
builder.Services.AddXperienceCommunityRedirects();
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
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<AdminIdentityOptions>(options =>
    {
        options.AuthenticationOptions.Mode = AdminAuthenticationMode.MaintainForms;
    });
}

builder.Services.AddOptions<BDO.GWT.Core.Extensions.AzureCognitiveSearchOptions>()
    .BindConfiguration("AzureCognitiveSearch");   // section name in appsettings.json

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

// Loads the administration redirect domain (AdminRedirectOptions) from a configuration file section
builder.Services.Configure<AdminRedirectOptions>(builder.Configuration.GetSection("AdminRedirectOptions"));

// Configures email sending via SendGrid for system emails
builder.Services.AddXperienceSystemSendGrid();
// Configures email sending via SendGrid for the fallback situation
builder.Services.AddXperienceSendGrid();

// Loads the SendGrid API keys from configuration file sections
builder.Services.ConfigureSystemSendGridClientOptions(builder.Configuration.GetSection("SystemSendGridClientOptions"));
builder.Services.Configure<SendGridClientOptions>(builder.Configuration.GetSection("FallbackSendGridClientOptions"));

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseAuthentication();

RegisterErrorPageMiddleware(app, builder.Configuration, app.Environment);
RegisterAdminRedirectRewriter(app, builder.Configuration, builder.Environment);

app.UseKentico();
app.UseMiddleware<RedirectMiddleware>();
app.Kentico().MapRoutes();
app.MapControllerRoute(
    name: "default",
    pattern: $"{{{WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY}}}/{{controller}}/{{action}}",
    constraints: new
    {
        controller = "Home"
    }
);
app.Run();

// --- Local functions ---

void RegisterErrorPageMiddleware(WebApplication app, IConfiguration config, IHostEnvironment env)
{
    bool errorPagesEnabled = bool.TryParse(config["EnableErrorPages"], out var result) && result;
    if (!errorPagesEnabled) return;

    bool isAdminSite = bool.TryParse(config["IsAdminSite"], out var isAdminSiteResult) && isAdminSiteResult;
    if (isAdminSite) return;

    if (!env.IsDevelopment())
    {
        app.UseWhen(
            context => !ShouldSkipErrorPages(context),
            branch => branch.UseExceptionHandler("/Error/500")
        );

        app.UseHsts();
    }

    app.UseWhen(
        context => !ShouldSkipErrorPages(context),
        branch => branch.UseStatusCodePagesWithReExecute("/Error/{0}")
    );
}

static bool ShouldSkipErrorPages(HttpContext context) =>
    IsEdgeHealthProbe(context) ||
    IsHealthCheckPath(context);

static bool IsEdgeHealthProbe(HttpContext context)
{
    var ua = context.Request.Headers.UserAgent.ToString();
    return ua.Contains("Edge Health Probe", StringComparison.OrdinalIgnoreCase)
        || ua.Contains("HealthCheck", StringComparison.OrdinalIgnoreCase);
}

static bool IsHealthCheckPath(HttpContext context)
{
    var path = context.Request.Path.Value ?? string.Empty;
    return path.StartsWith("/robots", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/ping", StringComparison.OrdinalIgnoreCase);
}

void RegisterAdminRedirectRewriter(WebApplication app, IConfiguration config, IHostEnvironment env)
{
    if (!ShouldRegisterAdminRedirect(env, config))
    {
        return;
    }

    app.UseRewriter(CreateAdminRedirectRewriteOptions());
}

static bool ShouldRegisterAdminRedirect(IHostEnvironment env, IConfiguration config) =>
    !env.IsDevelopment() &&
    !string.IsNullOrWhiteSpace(config["AdminRedirectOptions:RedirectDomain"]);

static RewriteOptions CreateAdminRedirectRewriteOptions() =>
    new RewriteOptions().Add(ApplyAdminRedirect);

static void ApplyAdminRedirect(RewriteContext context)
{
    var httpContext = context.HttpContext;
    var request = httpContext.Request;

    if (!request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    string adminDomain = httpContext.RequestServices
        .GetRequiredService<IOptions<AdminRedirectOptions>>()
        .Value.RedirectDomain;

    if (adminDomain.Equals(request.Host.ToString(), StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    httpContext.Response.Redirect(
        $"//{adminDomain}{request.Path}{request.QueryString}",
        permanent: false);

    context.Result = RuleResult.EndResponse;
}
