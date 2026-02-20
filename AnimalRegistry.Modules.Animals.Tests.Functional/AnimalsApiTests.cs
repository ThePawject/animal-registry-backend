using AnimalRegistry.Modules.Animals.Api.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[Collection("Sequential")]
public sealed class AnimalsApiTests(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";
    private static int _signatureCounter = 100;

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private static string NextSig()
    {
        return $"2024/{_signatureCounter++:D4}";
    }

    [Fact]
    public async Task Create_Get_List_Workflow_WithShelterRole()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var createdId = await factory.CreateAsync(NextSig(), "trans-123", "Integration", AnimalSpecies.Dog,
            AnimalSex.Male);
        var dto = await factory.GetAsync(createdId);

        dto.Name.Should().Be("Integration");

        var list = await factory.ListAsync();
        list.Items.Should().Contain(a => a.Id == createdId);
    }

    [Fact]
    public async Task List_ReturnsCreatedItems_WithShelterRole()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var id1 = await factory.CreateAsync(NextSig(), "t-1", "ListOne", AnimalSpecies.Cat, AnimalSex.Female);
        var id2 = await factory.CreateAsync(NextSig(), "t-2", "ListTwo", AnimalSpecies.Dog, AnimalSex.Male);

        var list = await factory.ListAsync();

        list.Items.Should().Contain(a => a.Id == id1 && a.Name == "ListOne");
        list.Items.Should().Contain(a => a.Id == id2 && a.Name == "ListTwo");
    }

    [Fact]
    public async Task List_WithKeyWordSearch_ReturnsMatchingItems()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var id1 = await factory.CreateAsync(NextSig(), "t-search-1", "SearchOne", AnimalSpecies.Cat,
            AnimalSex.Female);
        await factory.CreateAsync(NextSig(), "t-search-2", "SearchTwo", AnimalSpecies.Dog, AnimalSex.Male);

        var list = await factory.ListAsync("archone");

        list.Items.Should().ContainSingle(a => a.Id == id1 && a.Name == "SearchOne");
    }

    [Fact]
    public async Task List_WithKeyWordSearch_MatchesEventsAndDates()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);

        var animalId = await factory.CreateAsync(NextSig(), "t-event-1", "EventAnimal", AnimalSpecies.Dog,
            AnimalSex.Male);

        var request = new CreateAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = new DateTimeOffset(2024, 01, 15, 10, 0, 0, TimeSpan.Zero),
            Description = "tesT event description",
        };

        var client = Factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(animalId), request);
        response.EnsureSuccessStatusCode();

        var list = await factory.ListAsync("es event descript");

        list.Items.Should().ContainSingle(a => a.Id == animalId && a.Name == "EventAnimal");
    }

    [Fact]
    public async Task List_WithKeyWordSearch_Empty_ReturnsAll()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var id1 = await factory.CreateAsync(NextSig(), "t-empty-1", "Alpha", AnimalSpecies.Cat,
            AnimalSex.Female);
        var id2 = await factory.CreateAsync(NextSig(), "t-empty-2", "Beta", AnimalSpecies.Dog, AnimalSex.Male);

        var list = await factory.ListAsync("  ");

        list.Items.Should().Contain(a => a.Id == id1);
        list.Items.Should().Contain(a => a.Id == id2);
    }


    [Fact]
    public async Task WithoutShelterRole_ReturnsForbidden()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithoutShelterAccess());
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync(NextSig(), "t-forbidden", "Forbidden", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithoutAuthentication_ReturnsUnauthorized()
    {
        var factory = new AnimalFactory(new ApiClient(Client));

        var act = async () =>
            await factory.CreateAsync(NextSig(), "t-unauth", "Unauthorized", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithTwoShelterRoles_ReturnsForbidden()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithMultipleShelters("shelter-1", "shelter-2"));
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () => await factory.CreateAsync(NextSig(), "t-two", "Two", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithCustomRole_ReturnsForbidden()
    {
        var client = Factory.CreateAuthenticatedClient(TestUser.WithCustomRole("Admin"));
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync(NextSig(), "t-admin", "Admin", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithEmptyRoles_ReturnsForbidden()
    {
        var client = Factory.CreateAuthenticatedClient(new TestUser { Roles = [] });
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync(NextSig(), "t-empty", "Empty", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task WithWrongShelterPrefix_ReturnsForbidden()
    {
        var client = Factory.CreateAuthenticatedClient(new TestUser { Roles = ["WrongPrefix_123"] });
        var factory = new AnimalFactory(new ApiClient(client));

        var act = async () =>
            await factory.CreateAsync(NextSig(), "t-wrong", "Wrong", AnimalSpecies.Dog, AnimalSex.Male);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}