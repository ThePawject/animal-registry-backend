using AnimalRegistry.Modules.Animals.Api.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;
using System.Net.Http.Json;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.AnimalEvents;

[TestSubject(typeof(AddAnimalEvent))]
public class AddAnimalEventTest(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task ShouldAddEvent_WhenRequestIsValid()
    {
        // Arrange
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);

        var animalId = await factory.CreateAsync(
            "SIG-ADD-1",
            "TRANS-ADD-1",
            "Test Animal",
            AnimalSpecies.Dog,
            AnimalSex.Male);

        var request = new AddAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Test event description",
            PerformedBy = "Test User"
        };

        // Act
        var client = fixture.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync(AddAnimalEventRequest.BuildRoute(animalId), request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var animal = await factory.GetAsync(animalId);
        animal.Events.Should().ContainSingle();
        var addedEvent = animal.Events.First();
        addedEvent.Type.Should().Be(request.Type);
        addedEvent.Description.Should().Be(request.Description);
        addedEvent.PerformedBy.Should().Be(request.PerformedBy);
    }

    [Fact]
    public async Task ShouldFail_WhenUserHasDifferentShelterId()
    {
        // Arrange
        var ownerUser = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(ownerUser);

        var animalId = await factory.CreateAsync(
            "SIG-ADD-2",
            "TRANS-ADD-2",
            "Test Animal 2",
            AnimalSpecies.Cat,
            AnimalSex.Female);

        var otherUser = TestUser.WithShelterAccess("other-shelter");
        var otherClient = fixture.CreateAuthenticatedClient(otherUser);

        var request = new AddAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Unauthorized event",
            PerformedBy = "Intruder"
        };

        // Act
        var response = await otherClient.PostAsJsonAsync(AddAnimalEventRequest.BuildRoute(animalId), request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}