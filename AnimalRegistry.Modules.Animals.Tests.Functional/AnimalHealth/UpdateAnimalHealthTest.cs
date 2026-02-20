using AnimalRegistry.Modules.Animals.Api.AnimalHealth;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalHealth;

[TestSubject(typeof(UpdateAnimalHealth))]
[Collection("Sequential")]
public class UpdateAnimalHealthTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private static async Task AddHealthAsync(HttpClient client, Guid animalId, CreateAnimalHealthRequest request)
    {
        var response = await client.PostAsJsonAsync(CreateAnimalHealthRequest.BuildRoute(animalId), request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldUpdateHealthRecord_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9104",
            "TRANS-HEALTH-3",
            "Health Update Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var addRequest = new CreateAnimalHealthRequest
        {
            AnimalId = animalId, OccurredOn = DateTimeOffset.UtcNow.AddDays(-1), Description = "Original health",
        };
        await AddHealthAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var recordId = animal.HealthRecords.First().Id;

        var updateRequest = new UpdateAnimalHealthRequest
        {
            AnimalId = animalId,
            HealthRecordId = recordId,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Updated health",
        };

        var response = await client.PutAsJsonAsync(
            UpdateAnimalHealthRequest.BuildRoute(animalId, recordId),
            updateRequest);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedRecord = updatedAnimal.HealthRecords.First(r => r.Id == recordId);

        updatedRecord.Description.Should().Be(updateRequest.Description);
        updatedRecord.OccurredOn.Should().BeCloseTo(updateRequest.OccurredOn, TimeSpan.FromMilliseconds(100));
    }
}