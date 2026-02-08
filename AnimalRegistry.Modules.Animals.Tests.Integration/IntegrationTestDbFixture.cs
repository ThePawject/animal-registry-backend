using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Shared.DDD;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Integration;

public sealed class IntegrationTestDbFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("yourStrong(!)Password")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();
    public AnimalsDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<AnimalsDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        await ApplyMigrations(options, dispatcher);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    private async Task ApplyMigrations(DbContextOptions<AnimalsDbContext> options, IDomainEventDispatcher dispatcher)
    {
        DbContext = new AnimalsDbContext(options, dispatcher);
        await DbContext.Database.MigrateAsync();
    }
}