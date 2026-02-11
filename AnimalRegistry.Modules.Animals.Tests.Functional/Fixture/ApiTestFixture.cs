namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public class ApiTestFixture : IAsyncLifetime
{
    private DatabaseContainerFixture _dbFixture = null!;
    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _dbFixture = new DatabaseContainerFixture();
        await _dbFixture.InitializeAsync();

        Factory = new IntegrationTestWebApplicationFactory();
        Factory.ConnectionString = _dbFixture.ConnectionString;

        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _dbFixture.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}