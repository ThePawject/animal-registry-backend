using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public interface IAnimalDtoMapper
{
    AnimalDto Map(Animal animal);
    AnimalListItemDto MapToListItem(Animal animal);
}

public class AnimalDtoMapper(IBlobStorageService blobStorageService) : IAnimalDtoMapper
{
    public AnimalDto Map(Animal animal)
    {
        return new AnimalDto(
            animal.Id,
            animal.Signature,
            animal.TransponderCode,
            animal.Name,
            animal.Color,
            animal.Species,
            animal.Sex,
            animal.BirthDate,
            animal.CreatedOn,
            animal.ModifiedOn,
            animal.IsActive,
            animal.ShelterId,
            animal.MainPhotoId,
            animal.Photos.Select(MapPhoto).ToList(),
            animal.Events.Select(AnimalEventDto.FromDomain).ToList()
        );
    }

    public AnimalListItemDto MapToListItem(Animal animal)
    {
        return new AnimalListItemDto(
            animal.Id,
            animal.Signature,
            animal.TransponderCode,
            animal.Name,
            animal.Color,
            animal.Species,
            animal.Sex,
            animal.BirthDate,
            animal.CreatedOn,
            animal.ModifiedOn,
            animal.IsActive,
            animal.ShelterId,
            animal.MainPhotoId,
            animal.MainPhoto is not null ? MapPhoto(animal.MainPhoto) : null
        );
    }

    private AnimalPhotoDto MapPhoto(AnimalPhoto photo)
    {
        return new AnimalPhotoDto(
            photo.Id,
            blobStorageService.GetBlobUrl(photo.BlobPath),
            photo.FileName,
            photo.UploadedOn
        );
    }
}
