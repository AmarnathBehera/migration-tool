using BDO.GWT.GLOBAL.API;
using Kentico.Web.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddKentico();

builder.Services.AddAPIServices();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.InitKentico();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseKentico();

app.UseAuthorization();

app.MapControllers();

app.Run();
