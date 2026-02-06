using AnimalRegistry.Modules.Animals.Api.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalEvents;

[TestSubject(typeof(UpdateAnimalEvent))]
public class UpdateAnimalEventTest(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    private async Task AddEventAsync(HttpClient client, Guid animalId, AddAnimalEventRequest request)
    {
        var response = await client.PostAsJsonAsync(AddAnimalEventRequest.BuildRoute(animalId), request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldUpdateEvent_WhenRequestIsValid()
    {
        // Arrange
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = fixture.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "SIG-UPD-1",
            "TRANS-UPD-1",
            "Update Test Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var addRequest = new AddAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow.AddDays(-1),
            Description = "Original Description",
            PerformedBy = "Original User"
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
            PerformedBy = "Updated User"
        };

        // Act
        var response =
            await client.PutAsJsonAsync(UpdateAnimalEventRequest.BuildRoute(animalId, eventId), updateRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var updatedAnimal = await factory.GetAsync(animalId);
        var updatedEvent = updatedAnimal.Events.First(e => e.Id == eventId);

        updatedEvent.Type.Should().Be(updateRequest.Type);
        updatedEvent.Description.Should().Be(updateRequest.Description);
        updatedEvent.PerformedBy.Should().Be(updateRequest.PerformedBy);
        updatedEvent.OccurredOn.Should().BeCloseTo(updateRequest.OccurredOn, TimeSpan.FromMilliseconds(100));
    }
}