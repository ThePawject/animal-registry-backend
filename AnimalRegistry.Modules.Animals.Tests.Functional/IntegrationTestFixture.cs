using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net.Http.Headers;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .Build();

    private WebApplicationFactory<Program> Factory { get; set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    private TestJwtTokenGenerator TokenGenerator { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        TokenGenerator = new TestJwtTokenGenerator();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ConfigureTestJwtAuthentication(services);
                ConfigureTestDatabase(services);
                ConfigureTestBlobStorage(services);
                RunMigrations(services);
            });
        });

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    public HttpClient CreateAuthenticatedClient(TestUser user)
    {
        var client = Factory.CreateClient();
        var token = TokenGenerator.GenerateToken(user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private void ConfigureTestJwtAuthentication(IServiceCollection services)
    {
        var jwtDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(JwtBearerOptions));
        if (jwtDescriptor != null)
        {
            services.Remove(jwtDescriptor);
        }

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = TokenGenerator.GetTokenValidationParameters();
        });
    }

    private void ConfigureTestDatabase(IServiceCollection services)
    {
        var settingsDescriptor =
            services.FirstOrDefault(d => d.ServiceType == typeof(IConfigureOptions<AnimalsDatabaseSettings>));
        if (settingsDescriptor != null)
        {
            services.Remove(settingsDescriptor);
        }

        services.Configure<AnimalsDatabaseSettings>(options =>
        {
            options.ConnectionString = _dbContainer.GetConnectionString();
        });

        var dbDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AnimalsDbContext>));
        if (dbDescriptor != null)
        {
            services.Remove(dbDescriptor);
        }

        services.AddDbContext<AnimalsDbContext>((_, opts) =>
        {
            opts.UseSqlServer(_dbContainer.GetConnectionString());
        });
    }

    private static void ConfigureTestBlobStorage(IServiceCollection services)
    {
        var blobStorageDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBlobStorageService));
        if (blobStorageDescriptor != null)
        {
            services.Remove(blobStorageDescriptor);
        }

        var mockBlobStorage = Substitute.For<IBlobStorageService>();
        mockBlobStorage.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult($"http://test-blob-storage/{Guid.NewGuid()}_{ci.ArgAt<string>(0)}"));

        services.AddSingleton(mockBlobStorage);
    }

    private static void RunMigrations(IServiceCollection services)
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnimalsDbContext>();
        context.Database.Migrate();
    }
}