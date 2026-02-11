using AnimalRegistry.Modules.Animals.Api.AnimalHealth;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalHealth;

[TestSubject(typeof(CreateAnimalHealth))]
[Collection("Sequential")]
public class CreateAnimalHealthTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task ShouldAddHealthRecord_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);

        var animalId = await factory.CreateAsync(
            "2024/9101",
            "TRANS-HEALTH-1",
            "Health Animal",
            AnimalSpecies.Dog,
            AnimalSex.Male);

        var request = new CreateAnimalHealthRequest
        {
            AnimalId = animalId, OccurredOn = DateTimeOffset.UtcNow, Description = "Initial health record",
        };

        var client = Factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync(CreateAnimalHealthRequest.BuildRoute(animalId), request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var animal = await factory.GetAsync(animalId);
        animal.HealthRecords.Should().ContainSingle();
        var record = animal.HealthRecords.First();
        record.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task ShouldFail_WhenUserHasDifferentShelterId()
    {
        var ownerUser = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(ownerUser);

        var animalId = await factory.CreateAsync(
            "2024/9102",
            "TRANS-HEALTH-2",
            "Health Animal 2",
            AnimalSpecies.Cat,
            AnimalSex.Female);

        var otherUser = TestUser.WithShelterAccess("other-shelter");
        var otherClient = Factory.CreateAuthenticatedClient(otherUser);

        var request = new CreateAnimalHealthRequest
        {
            AnimalId = animalId, OccurredOn = DateTimeOffset.UtcNow, Description = "Unauthorized health record",
        };

        var response = await otherClient.PostAsJsonAsync(CreateAnimalHealthRequest.BuildRoute(animalId), request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}