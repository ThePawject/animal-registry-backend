using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

[TestSubject(typeof(DeleteAnimal))]
[Collection("Sequential")]
public class DeleteAnimalTest(ApiTestFixture fixture) : IntegrationTestBase(fixture)
{
    private const string TestShelterId = "test-shelter-1";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = Factory.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task ShouldDeleteAnimal_WhenRequestIsValid()
    {
        var user = TestUser.WithShelterAccess(TestShelterId);
        var factory = CreateFactory(user);
        var client = Factory.CreateAuthenticatedClient(user);

        var animalId = await factory.CreateAsync(
            "2024/9401",
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
            "2024/9402",
            "TRANS-DEL-2",
            "Other Shelter Animal",
            AnimalSpecies.Dog,
            AnimalSex.Female);

        var otherUser = TestUser.WithShelterAccess("other-shelter");
        var otherClient = Factory.CreateAuthenticatedClient(otherUser);

        var response = await otherClient.DeleteAsync(DeleteAnimalRequest.BuildRoute(animalId));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}