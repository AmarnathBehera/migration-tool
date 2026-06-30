using BDO.GWT.GLOBAL.API;
using Kentico.Web.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddKentico();

builder.Services.AddAPIServices();
//builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "GWT API", Version = "v2" });
   
    // Security Definition
    c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization,
        Description = "JWT bearer token.",
    });

    // Security Requirement (UPDATED CLEAN SYNTAX)
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

    foreach (var assemblyName in new[] { "BDO.GWT.GLOBAL.API", "BDO.GWT.Core" })
    {
        c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml"), true);
    }
});

//builder.Services.AddSingleton<IMapper>(_ => new Mapper(ApiAutoMapperConfiguration.MapperConfiguration));

var app = builder.Build();
app.InitKentico();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
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

app.UseHttpsRedirection();

app.UseKentico();

app.UseAuthorization();

app.MapControllers();

app.Run();
