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

    [Fact]
    public async Task AnimalPhotos_Upload_Multiple_WithMainIndex_WorksCorrectly()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));
        var photo1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        var photo2 = new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var photos = new List<(string, byte[], string)>
        {
            ("dog1.jpg", photo1, "image/jpeg"),
            ("dog2.jpg", photo2, "image/jpeg"),
        };
        var animalId = await factory.CreateAsync(
            "sig-photo-main2", "trans-photo-main2", "MainDog", AnimalSpecies.Dog, AnimalSex.Female, photos, 1);
        var dto = await factory.GetAsync(animalId);

        dto.Photos.Should().NotBeNullOrEmpty();
        dto.Photos.Count.Should().Be(2);
        
        dto.MainPhotoId.Should().NotBeNull();
        
        var mainPhoto = dto.Photos.Single(p => p.Id == dto.MainPhotoId);
        var otherPhoto = dto.Photos.Single(p => p.Id != dto.MainPhotoId);
        
        mainPhoto.FileName.Should().Be("dog2.jpg");
        otherPhoto.FileName.Should().Be("dog1.jpg");
        mainPhoto.Url.Should().NotBeNullOrEmpty();
        otherPhoto.Url.Should().NotBeNullOrEmpty();
    }
}