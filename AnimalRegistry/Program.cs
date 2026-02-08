global using AnimalRegistry;
using AnimalRegistry.Modules.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

var modules = new List<IModule> { new AnimalsModule() };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAuthorizationHandler, ShelterAccessHandler>();

builder.Services.AddAuth0OpenApi(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddPagination(builder.Configuration);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var domain = builder.Configuration["Auth0:Domain"];
var audience = builder.Configuration["Auth0:Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = audience;

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "https://ThePawject/roles",
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ShelterAccessHandler.ShelterAccessPolicyName, policy =>
        policy.Requirements.Add(new ShelterAccessRequirement()));
});
ApplyModuleServices(modules, builder);

var app = builder.Build();

app.UseBusinessRuleExceptionHandling();
app.UseDefaultExceptionHandler();

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.LogCompleteSecurityArtifact = true;
    IdentityModelEventSource.ShowPII = true;
}
app.MapOpenApi();
app.MapScalarWithAuth0(builder.Configuration);

app.UseHttpsRedirection();
app.UseCors();
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