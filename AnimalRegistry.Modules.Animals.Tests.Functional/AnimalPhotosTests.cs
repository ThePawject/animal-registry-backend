using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class AnimalPhotosTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string TestShelterId = "test-shelter-photos";

    private AnimalFactory CreateFactory(TestUser user)
    {
        var client = fixture.CreateAuthenticatedClient(user);
        return new AnimalFactory(new ApiClient(client));
    }

    [Fact]
    public async Task AnimalPhotos_AreOptional()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var animalId1 = await factory.CreateAsync("sig-opt-1", "trans-opt-1", "Optional1", AnimalSpecies.Dog,
            AnimalSex.Male);
        var animalId2 = await factory.CreateAsync("sig-opt-2", "trans-opt-2", "Optional2", AnimalSpecies.Cat,
            AnimalSex.Female);

        var listResult = await factory.ListAsync();

        listResult.Items.Should().Contain(a => a.Id == animalId1);
        listResult.Items.Should().Contain(a => a.Id == animalId2);
    }
}
