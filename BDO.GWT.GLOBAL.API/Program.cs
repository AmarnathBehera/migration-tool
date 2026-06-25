using Kentico.Web.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Add services to the container.

// Register Kentico Xperience services. This is required because the project
// (transitively) references Kentico assemblies whose MVC filters (e.g.
// LogExceptionMvcFilterAttribute) resolve services from the Kentico IoC
// container during controller discovery. Without AddKentico/InitKentico the
// container is not initialized and MapControllers() fails at startup.
builder.Services.AddKentico();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize the Kentico IoC container before any controller/endpoint
// discovery runs.
app.InitKentico();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseKentico();

app.UseAuthorization();

app.MapControllers();

app.Run();
