using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[TestSubject(typeof(DeleteAnimal))]
public class DeleteAnimalTest(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task ShouldDeleteAnimal_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = fixture.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "SIG-DEL-1",
            "TRANS-DEL-1",
            "Delete Test Animal",
            AnimalSpecies.Cat,
            AnimalSex.Male);

        var response = await client.DeleteAsync(DeleteAnimalRequest.BuildRoute(animalId));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync(GetAnimalRequest.BuildRoute(animalId));
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldFail_WhenUserHasDifferentShelterId()
    {
        var ownerUser = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(ownerUser);

        var animalId = await factory.CreateAsync(
            "SIG-DEL-2",
            "TRANS-DEL-2",
            "Other Shelter Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var otherUser = TestUser.WithShelterAccess("other-shelter");
        var otherClient = fixture.CreateAuthenticatedClient(otherUser);

        var response = await otherClient.DeleteAsync(DeleteAnimalRequest.BuildRoute(animalId));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}