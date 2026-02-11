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

[TestSubject(typeof(CreateAnimalEvent))]
[Collection("Sequential")]
public class CreateAnimalEventTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task ShouldAddEvent_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);

        var animalId = await factory.CreateAsync(
            "2024/9001",
            "TRANS-ADD-1",
            "Test Animal",
            AnimalSpecies.Dog,
            AnimalSex.Male);

        var request = new CreateAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Test event description",
        };

        var client = Factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(animalId), request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var animal = await factory.GetAsync(animalId);
        animal.Events.Should().ContainSingle();
        var addedEvent = animal.Events.First();
        addedEvent.Type.Should().Be(request.Type);
        addedEvent.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task ShouldFail_WhenUserHasDifferentShelterId()
    {
        var ownerUser = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(ownerUser);

        var animalId = await factory.CreateAsync(
            "2024/9002",
            "TRANS-ADD-2",
            "Test Animal 2",
            AnimalSpecies.Cat,
            AnimalSex.Female);

        var otherUser = TestUser.WithShelterAccess("other-shelter");
        var otherClient = Factory.CreateAuthenticatedClient(otherUser);

        var request = new CreateAnimalEventRequest
        {
            AnimalId = animalId,
            Type = AnimalEventType.AdmissionToShelter,
            OccurredOn = DateTimeOffset.UtcNow,
            Description = "Unauthorized event",
        };

        var response = await otherClient.PostAsJsonAsync(CreateAnimalEventRequest.BuildRoute(animalId), request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}