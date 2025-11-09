using AnimalRegistry.Modules.Animals;
using AnimalRegistry.Shared;
using FastEndpoints;
using Scalar.AspNetCore;

var modules = new List<IModule> { new AnimalsModule() };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

ApplyModuleServices(modules, builder);

var app = builder.Build();

app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.Run();
return;

void ApplyModuleServices(List<IModule> list, WebApplicationBuilder webApplicationBuilder)
{
    foreach (var module in list)
    {
        module.RegisterServices(webApplicationBuilder.Services, webApplicationBuilder.Configuration);
    }
}