using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Constants;
using BDO.GWT.GLOBAL.API.Extensions;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace BDO.GWT.GLOBAL.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddKentico();
        builder.Services.AddApi();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

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

        app.UseAuthorization();

        app.UseMiddleware<Middleware.MemberFirmMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
