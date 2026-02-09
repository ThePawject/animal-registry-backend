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
            ("dog1.jpg", photo1, "image/jpeg"), ("dog2.jpg", photo2, "image/jpeg"),
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

    [Fact]
    public async Task UpdateAnimal_WithPhotoManagement_RemovesAddsAndChangesMain()
    {
        var factory = CreateFactory(TestUser.WithShelterAccess(TestShelterId));

        var photo1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        var photo2 = new byte[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var photos = new List<(string, byte[], string)>
        {
            ("initial1.jpg", photo1, "image/jpeg"), ("initial2.jpg", photo2, "image/jpeg"),
        };

        var animalId = await factory.CreateAsync(
            "sig-update-photo", "trans-update-photo", "UpdateDog", AnimalSpecies.Dog, AnimalSex.Male, photos, 0);

        var dtoBeforeUpdate = await factory.GetAsync(animalId);
        dtoBeforeUpdate.Photos.Count.Should().Be(2);
        dtoBeforeUpdate.MainPhotoId.Should().NotBeNull();

        var firstPhotoId = dtoBeforeUpdate.Photos.First().Id;
        var secondPhotoId = dtoBeforeUpdate.Photos.Last().Id;
        dtoBeforeUpdate.MainPhotoId.Should().Be(firstPhotoId);

        var newPhoto = new byte[] { 10, 20, 30, 40, 50 };
        var newPhotos = new List<(string, byte[], string)> { ("new1.jpg", newPhoto, "image/jpeg") };

        await factory.UpdateAsync(
            animalId,
            "sig-updated",
            "trans-updated",
            "UpdatedDog",
            AnimalSpecies.Dog,
            AnimalSex.Female,
            [secondPhotoId],
            newPhotos,
            null,
            0);

        var dtoAfterUpdate = await factory.GetAsync(animalId);

        dtoAfterUpdate.Name.Should().Be("UpdatedDog");
        dtoAfterUpdate.Signature.Should().Be("sig-updated");
        dtoAfterUpdate.TransponderCode.Should().Be("trans-updated");
        dtoAfterUpdate.Sex.Should().Be(AnimalSex.Female);
        dtoAfterUpdate.Color.Should().Be("UpdatedColor");

        dtoAfterUpdate.Photos.Count.Should().Be(2);
        dtoAfterUpdate.Photos.Should().Contain(p => p.Id == secondPhotoId);
        dtoAfterUpdate.Photos.Should().NotContain(p => p.Id == firstPhotoId);

        var newlyAddedPhoto = dtoAfterUpdate.Photos.Single(p => p.Id != secondPhotoId);
        newlyAddedPhoto.FileName.Should().Be("new1.jpg");

        dtoAfterUpdate.MainPhotoId.Should().Be(newlyAddedPhoto.Id);
    }
}