namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly FunctionalTestWebApplicationFactory Factory;

    protected IntegrationTestBase(ApiTestFixture fixture)
    {
        Factory = fixture.Factory;
        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return Task.CompletedTask;
    }
}