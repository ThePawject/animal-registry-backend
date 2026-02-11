using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Respawn;
using System.Data.Common;
using System.Net.Http.Headers;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;
    public string ConnectionString { get; set; } = string.Empty;
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

            var blobStorageDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IBlobStorageService));
            if (blobStorageDescriptor != null)
            {
                services.Remove(blobStorageDescriptor);
            }

            var mockBlobStorage = Substitute.For<IBlobStorageService>();
            mockBlobStorage.UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Guid>(),
                    Arg.Any<CancellationToken>())
                .Returns(ci => Task.FromResult(Result<string>.Success(
                    $"test-shelter/{Guid.NewGuid()}/{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{ci.ArgAt<string>(0)}")));

            mockBlobStorage.GetBlobUrl(Arg.Any<string>())
                .Returns(ci => $"http://test-blob-storage/{ci.ArgAt<string>(0)}");

            services.AddSingleton(mockBlobStorage);

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