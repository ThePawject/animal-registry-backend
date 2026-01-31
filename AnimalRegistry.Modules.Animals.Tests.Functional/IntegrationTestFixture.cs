using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(Microsoft.Extensions.Options.IConfigureOptions<AnimalRegistry.Modules.Animals.Infrastructure.AnimalsDatabaseSettings>));
                if (descriptor != null) services.Remove(descriptor);

                services.Configure<AnimalRegistry.Modules.Animals.Infrastructure.AnimalsDatabaseSettings>(options =>
                {
                    options.ConnectionString = _dbContainer.GetConnectionString();
                });

                var dbDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AnimalRegistry.Modules.Animals.Infrastructure.AnimalsDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<AnimalRegistry.Modules.Animals.Infrastructure.AnimalsDbContext>((sp, opts) =>
                {
                    opts.UseSqlServer(_dbContainer.GetConnectionString());
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<AnimalRegistry.Modules.Animals.Infrastructure.AnimalsDbContext>();
                ctx.Database.Migrate();
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
}
