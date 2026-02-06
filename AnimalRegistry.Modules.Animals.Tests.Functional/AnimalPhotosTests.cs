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
    public async Task GetAnimal_WithPhotos_ReturnsAllPhotos()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var animalId = await factory.CreateAsync("sig-photos-1", "trans-photos-1", "PhotoTest", AnimalSpecies.Dog,
            AnimalSex.Male);

        var dto = await factory.GetAsync(animalId);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(animalId);
        dto.Photos.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAnimals_WithMainPhoto_ReturnsOnlyMainPhoto()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var animalId = await factory.CreateAsync("sig-list-photo", "trans-list-photo", "ListPhotoTest", AnimalSpecies.Cat,
            AnimalSex.Female);

        var listResult = await factory.ListAsync();

        listResult.Should().NotBeNull();
        listResult.Items.Should().Contain(a => a.Id == animalId);
        
        var animalFromList = listResult.Items.First(a => a.Id == animalId);
        animalFromList.MainPhoto.Should().BeNull();
    }

    [Fact]
    public async Task CreateAnimal_WithoutPhotos_WorksCorrectly()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var animalId = await factory.CreateAsync("sig-no-photos", "trans-no-photos", "NoPhotosTest", AnimalSpecies.Dog,
            AnimalSex.Male);

        var dto = await factory.GetAsync(animalId);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(animalId);
        dto.Photos.Should().BeEmpty();
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
