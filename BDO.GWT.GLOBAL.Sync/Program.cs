using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Services;
using Kentico.Web.Mvc;
using Microsoft.OpenApi;

namespace BDO.GWT.GLOBAL.Sync;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        const bool enableTemporarySwagger = true;

        builder.Services.AddControllersWithViews();
        builder.Services.AddKentico();
        builder.Services.AddContentServices();
        builder.Services.AddApiService(builder.Configuration);
        builder.Services.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
        builder.Services.AddHealthChecks();

        // Temporary Swagger block: set enableTemporarySwagger = false or remove this block later.
        if (enableTemporarySwagger)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BDO GWT GLOBAL Sync API",
                    Version = "v1"
                });
            });
        }

        var app = builder.Build();
        app.InitKentico();

        if (enableTemporarySwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "BDO GWT GLOBAL Sync API v1");
            });
        }

        app.MapHealthChecks("/healthz");

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}