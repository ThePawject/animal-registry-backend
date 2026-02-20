global using AnimalRegistry;
using AnimalRegistry.Modules.Animals;
using AnimalRegistry.Modules.Audit;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.Outbox.Extensions;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

var modules = new List<IModule> { new AnimalsModule(), new AuditModule() };

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.AddSingleton<IAuthorizationHandler, ShelterAccessHandler>();

builder.Services.AddAuth0OpenApi(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddPagination(builder.Configuration);
builder.Services.AddOutbox(builder.Configuration);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition")
    );
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

await MigrateAsync(app, modules);

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

async Task MigrateAsync(WebApplication webApplication, List<IModule> modules1)
{
    var logger = webApplication.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting database migrations for {Count} modules...", modules1.Count);

    foreach (var module in modules1)
    {
        logger.LogInformation("Migrating module: {ModuleName}", module.Name);
        await module.MigrateAsync(webApplication.Services);
        logger.LogInformation("Module {ModuleName} migrated successfully", module.Name);
    }

    logger.LogInformation("All database migrations completed successfully");
}

public partial class Program
{
}