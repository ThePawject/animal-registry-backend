using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalListItemDto(
    Guid Id,
    string Signature,
    string TransponderCode,
    string Name,
    string Color,
    AnimalSpecies Species,
    AnimalSex Sex,
    DateTimeOffset BirthDate,
    DateTimeOffset CreatedOn,
    DateTimeOffset ModifiedOn,
    bool IsInShelter,
    string ShelterId,
    Guid? MainPhotoId,
    AnimalPhotoDto? MainPhoto
)
{
    public static AnimalListItemDto FromDomain(Animal a)
    {
        return new AnimalListItemDto(
            a.Id,
            a.Signature,
            a.TransponderCode,
            a.Name,
            a.Color,
            a.Species,
            a.Sex,
            a.BirthDate,
            a.CreatedOn,
            a.ModifiedOn,
            a.IsInShelter,
            a.ShelterId,
            a.MainPhotoId,
            a.MainPhoto is not null ? new AnimalPhotoDto(
                a.MainPhoto.Id,
                a.MainPhoto.Url ?? string.Empty,
                a.MainPhoto.FileName,
                a.MainPhoto.UploadedOn
            ) : null
        );
    }
}
