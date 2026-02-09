using AnimalRegistry.Modules.Animals.Api.AnimalHealth;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalHealth;

[TestSubject(typeof(DeleteAnimalHealth))]
public class DeleteAnimalHealthTest(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private async Task AddHealthAsync(HttpClient client, Guid animalId, CreateAnimalHealthRequest request)
    {
        var response = await client.PostAsJsonAsync(CreateAnimalHealthRequest.BuildRoute(animalId), request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldDeleteHealthRecord_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = fixture.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "SIG-HEALTH-4",
            "TRANS-HEALTH-4",
            "Health Delete Animal",
            AnimalSpecies.Cat,
            AnimalSex.Male);

        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId, OccurredOn = DateTimeOffset.UtcNow, Description = "To be deleted"
        };
        await AddHealthAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        var response = await client.DeleteAsync(DeleteAnimalHealthRequest.BuildRoute(animalId, recordId));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        updatedAnimal.HealthRecords.Should().BeEmpty();
    }
}