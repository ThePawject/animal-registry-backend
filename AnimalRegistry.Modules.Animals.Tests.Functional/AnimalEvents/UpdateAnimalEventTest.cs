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

[TestSubject(typeof(UpdateAnimalEvent))]
[Collection("Sequential")]
public class UpdateAnimalEventTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
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
    public async Task ShouldUpdateEvent_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9301",
            "TRANS-UPD-1",
            "Update Test Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var addRequest = new CreateAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Original Description",
        };
        await AddEventAsync(client, animalId, addRequest);

        var animal = await factory.GetAsync(animalId);
        var eventId = animal.Events.First().Id;

        var updateRequest = new UpdateAnimalEventRequest
        {
            AnimalId = animalId,
            EventId = eventId,
            Type = AnimalEventType.StartOfQuarantine,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Updated Description",
        };

        var response =
            await client.PutAsJsonAsync(UpdateAnimalEventRequest.BuildRoute(animalId, eventId), updateRequest);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedEvent = updatedAnimal.Events.First(e => e.Id == eventId);

        updatedEvent.Type.Should().Be(updateRequest.Type);
        updatedEvent.Description.Should().Be(updateRequest.Description);
        updatedEvent.OccurredOn.Should().BeCloseTo(updateRequest.OccurredOn, TimeSpan.FromMilliseconds(100));
    }
}