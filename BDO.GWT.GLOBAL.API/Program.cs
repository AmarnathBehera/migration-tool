using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.API.Constants;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.Services;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API;
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
        builder.Services.AddKentico();
        builder.Services.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
        builder.Services.AddContentServices();
        builder.Services.AddApi();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        var azureAdSection = builder.Configuration.GetSection("AzureAd");
        var azureAdClientId = azureAdSection["ClientId"];
        var azureAdAudience = azureAdSection["Audience"];

        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
        {
            // Accept both v1 (aud = App ID URI) and v2 (aud = client id) access tokens.
            var validAudiences = new[]
            {
                azureAdAudience,
                azureAdClientId,
                $"api://{azureAdClientId}",
            }.Where(audience => !string.IsNullOrWhiteSpace(audience)).Distinct().ToArray();

            jwtBearerOptions.TokenValidationParameters.ValidAudiences = validAudiences;

            jwtBearerOptions.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var environment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

                    var hasBearerToken = context.Request.Headers.Authorization
                        .Any(value => value is not null && value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

                    // Dev convenience: skip validation until a bearer token is supplied via Swagger.
                    if (environment.IsDevelopment() && !hasBearerToken)
                    {
                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "SwaggerNoAuth"));
                        context.Success();
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearerAuthentication")
                        .LogWarning(context.Exception, "JWT authentication failed for {RequestPath}.", context.Request.Path);

                    return Task.CompletedTask;
                },
            };
        });

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v2", new OpenApiInfo { Title = "GWT API", Version = "v2" });

            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = HeaderNames.Authorization,
                Description = "JWT bearer token.",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "bearerAuth",
                    },
                }] = Array.Empty<string>(),
            });

            c.EnableAnnotations();

            foreach (var assemblyName in new[] { ProjectConstants.gwtGlobalAPI, ProjectConstants.gwtCore })
            {
                c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml"), true);
            }
        });

        var app = builder.Build();
        app.InitKentico();

        if (app.Environment.IsDevelopment())
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.ConfigObject.AdditionalItems["filter"] = true;
                options.ConfigObject.AdditionalItems["tagsSorter"] = "alpha";
                options.ConfigObject.DisplayRequestDuration = true;
                options.ConfigObject.PersistAuthorization = true;
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "GWT API");
            });
        }

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                switch (exception)
                {
                    case UnprocessableEntityException ex:
                        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = ex.Message
                        });
                        break;
                    case NotFoundException ex:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = ex.Message
                        });
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        break;
                }
            });
        });
        app.UseHttpsRedirection();

        app.UseKentico();

        app.UseAuthentication();

        app.UseMiddleware<Middleware.MemberFirmMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}