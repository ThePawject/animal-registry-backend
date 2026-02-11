using AnimalRegistry.Modules.Animals.Api.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalEvents;

[TestSubject(typeof(DeleteAnimalEvent))]
[Collection("Sequential")]
public class DeleteAnimalEventTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private async Task AddEventAsync(HttpClient client, Guid animalId, CreateAnimalEventRequest request)
    {
        var response = await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(animalId), request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldDeleteEvent_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9201",
            "TRANS-DEL-1",
            "Delete Test Animal",
            AnimalSpecies.Cat,
            AnimalSex.Male);

        var addRequest = new CreateAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "To be deleted",
        };
        await AddEventAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var eventId = animal.Events.First().Id;

        var response = await client.DeleteAsync(DeleteAnimalEventRequest.BuildRoute(animalId, eventId));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        updatedAnimal.Events.Should().BeEmpty();
    }
}