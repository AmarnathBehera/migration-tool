using BDO.GWT.GLOBAL.API.Services;


namespace BDO.GWT.GLOBAL.API;

public static class ServiceCollectionExtensions
{
    public static void AddAPIServices(this IServiceCollection serviceCollection)
    {
       serviceCollection.AddScoped<IFormCrudService, FormCrudService>();
    }

}
