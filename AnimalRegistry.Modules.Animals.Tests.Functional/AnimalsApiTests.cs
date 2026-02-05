using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class AnimalsApiTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task Create_Get_List_Workflow_WithShelterRole()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var createdId = await factory.CreateAsync("sig-integ-1", "trans-123", "Integration", AnimalSpecies.Dog,
            AnimalSex.Male);
        var dto = await factory.GetAsync(createdId);

        dto.Name.Should().Be("Integration");

        var list = await factory.ListAsync();
        list.Should().Contain(a => a.Id == createdId);
    }

    [Fact]
    public async Task List_ReturnsCreatedItems_WithShelterRole()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var id1 = await factory.CreateAsync("sig-list-1", "t-1", "ListOne", AnimalSpecies.Cat, AnimalSex.Female);
        var id2 = await factory.CreateAsync("sig-list-2", "t-2", "ListTwo", AnimalSpecies.Dog, AnimalSex.Male);

        var list = await factory.ListAsync();

        list.Should().Contain(a => a.Id == id1 && a.Name == "ListOne");
        list.Should().Contain(a => a.Id == id2 && a.Name == "ListTwo");
    }

    [Fact]
    public async Task WithoutShelterRole_ReturnsForbidden()
    {
        var client = fixture.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync("sig-forbidden", "t-forbidden", "Forbidden", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithoutAuthentication_ReturnsUnauthorized()
    {
        var factory = new AnimalFactory(new ApiClient(fixture.Client));

        var act = async () =>
            await factory.CreateAsync("sig-unauth", "t-unauth", "Unauthorized", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithTwoShelterRoles_ReturnsForbidden()
    {
        var client = fixture.CreateAuthenticatedClient(TestUser.WithMultipleShelters("shelter-1", "shelter-2"));
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () => await factory.CreateAsync("sig-two", "t-two", "Two", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithCustomRole_ReturnsForbidden()
    {
        var client = fixture.CreateAuthenticatedClient(TestUser.WithCustomRole("Admin"));
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync("sig-admin", "t-admin", "Admin", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithEmptyRoles_ReturnsForbidden()
    {
        var client = fixture.CreateAuthenticatedClient(new TestUser { Roles = [] });
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync("sig-empty", "t-empty", "Empty", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithWrongShelterPrefix_ReturnsForbidden()
    {
        var client = fixture.CreateAuthenticatedClient(new TestUser { Roles = ["WrongPrefix_123"] });
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync("sig-wrong", "t-wrong", "Wrong", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}