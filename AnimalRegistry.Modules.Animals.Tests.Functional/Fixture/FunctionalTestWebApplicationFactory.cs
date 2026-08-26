using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Contact.Infrastructure;
using AnimalRegistry.Modules.Contact.Infrastructure.Email;
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

    public RecordingEmailSender EmailSender { get; } = new();
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
        await _dbConnection.DisposeAsync();

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

            var contactDbSettingsDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IConfigureOptions<ContactDatabaseSettings>));
            if (contactDbSettingsDescriptor != null)
            {
                services.Remove(contactDbSettingsDescriptor);
            }

            services.Configure<ContactDatabaseSettings>(options =>
            {
                options.ConnectionString = ConnectionString;
            });

            services.Configure<EmailSettings>(options =>
            {
                options.Enabled = true;
                options.Host = "localhost";
                options.Port = 587;
                options.UserName = null;
                options.Password = null;
                options.FromAddress = "noreply@test.local";
                options.FromDisplayName = "MojeSchronisko";
                options.ContactRecipient = "team@test.local";
                options.TimeoutSeconds = 5;
            });

            var emailSenderDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailSenderDescriptor != null)
            {
                services.Remove(emailSenderDescriptor);
            }

            services.AddSingleton<IEmailSender>(EmailSender);

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
        EmailSender.Reset();
        await _respawner.ResetAsync(_dbConnection);
    }
}