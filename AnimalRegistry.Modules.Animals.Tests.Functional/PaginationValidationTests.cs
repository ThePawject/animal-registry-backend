using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Shared.Pagination;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class PaginationValidationTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-paging";

    private HttpClient CreateAuthenticatedClient()
    {
        return fixture.CreateAuthenticatedClient(TestUser.WithShelterAccess(TestShelterId));
    }

    [Theory]
    [InlineData(1, 20, HttpStatusCode.OK)]
    [InlineData(1, 1, HttpStatusCode.OK)]
    [InlineData(1, 100, HttpStatusCode.OK)]
    [InlineData(10, 50, HttpStatusCode.OK)]
    public async Task ListAnimals_WithValidPagination_ReturnsOk(int page, int pageSize, HttpStatusCode expectedStatus)
    {
        var client = CreateAuthenticatedClient();
        var api = new ApiClient(client);

        var resp = await api.GetAsync($"{ListAnimalsRequest.Route}?Page={page}&PageSize={pageSize}");

        resp.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    [InlineData(1, 1000)]
    public async Task ListAnimals_WithInvalidPagination_ReturnsBadRequest(int page, int pageSize)
    {
        var client = CreateAuthenticatedClient();
        var api = new ApiClient(client);

        var resp = await api.GetAsync($"{ListAnimalsRequest.Route}?Page={page}&PageSize={pageSize}");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListAnimals_WithoutQueryParams_UsesDefaults()
    {
        var client = CreateAuthenticatedClient();
        var api = new ApiClient(client);

        var resp = await api.GetAsync(ListAnimalsRequest.Route);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
