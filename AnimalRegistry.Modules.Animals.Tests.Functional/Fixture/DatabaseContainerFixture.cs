using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public sealed class DatabaseContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("SQLCMDUSER", "sa")
        .WithPassword("Stronk(!)Password")
        .WithPortBinding(1433, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-C", "-Q", "SELECT 1;"))
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}