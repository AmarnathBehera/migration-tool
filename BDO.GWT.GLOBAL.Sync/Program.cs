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

        builder.Services.AddControllersWithViews();
        builder.Services.AddKentico();
        builder.Services.AddContentServices();
        builder.Services.AddApiService(builder.Configuration);
        builder.Services.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
        builder.Services.AddHealthChecks();

        var app = builder.Build();
        app.InitKentico();

        app.MapHealthChecks("/healthz");

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}