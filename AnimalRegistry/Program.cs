using AnimalRegistry.Modules.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var modules = new List<IModule> { new AnimalsModule() };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var domain = builder.Configuration["Auth0:Domain"];
var audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = domain;
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

ApplyModuleServices(modules, builder);

var app = builder.Build();

app.UseBusinessRuleExceptionHandling();
app.UseDefaultExceptionHandler();

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.LogCompleteSecurityArtifact = true;
    IdentityModelEventSource.ShowPII = true; 
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();
return;

void ApplyModuleServices(List<IModule> list, WebApplicationBuilder webApplicationBuilder)
{
    foreach (var module in list)
    {
        module.RegisterServices(webApplicationBuilder.Services, webApplicationBuilder.Configuration);
    }
}

public partial class Program
{
}