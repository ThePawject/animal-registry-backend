using AnimalRegistry.Modules.Animals.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Respawn;
using System.Data.Common;
using System.Net.Http.Headers;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public sealed class FunctionalTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;
    public string ConnectionString { get; set; } = string.Empty;
    public string BlobStorageConnectionString { get; set; } = string.Empty;
    private TestJwtTokenGenerator TokenGenerator { get; set; } = null!;

    public async Task InitializeAsync()
    {
        TokenGenerator = new TestJwtTokenGenerator();

        _dbConnection = new SqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection,
            new RespawnerOptions { DbAdapter = DbAdapter.SqlServer, SchemasToInclude = ["dbo"] });
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbSettingsDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IConfigureOptions<AnimalsDatabaseSettings>));
            if (dbSettingsDescriptor != null)
            {
                services.Remove(dbSettingsDescriptor);
            }

            var dbContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AnimalsDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.Configure<AnimalsDatabaseSettings>(options =>
            {
                options.ConnectionString = ConnectionString;
            });

            services.AddDbContext<AnimalsDbContext>((_, opts) =>
            {
                opts.UseSqlServer(ConnectionString);
            });

            var blobStorageSettingsDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IConfigureOptions<BlobStorageSettings>));
            if (blobStorageSettingsDescriptor != null)
            {
                services.Remove(blobStorageSettingsDescriptor);
            }

            services.Configure<BlobStorageSettings>(options =>
            {
                options.ConnectionString = BlobStorageConnectionString;
                options.ContainerName = "test-animals";
                options.AccountName = "devstoreaccount1";
            });

            var jwtDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>));
            if (jwtDescriptor != null)
            {
                services.Remove(jwtDescriptor);
            }

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = TokenGenerator.GetTokenValidationParameters();
            });
        });
    }

    public HttpClient CreateAuthenticatedClient(TestUser user)
    {
        var client = CreateClient();
        var token = TokenGenerator.GenerateToken(user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}