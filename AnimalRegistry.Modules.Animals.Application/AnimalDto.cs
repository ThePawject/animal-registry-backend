using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalDto(
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
    bool IsActive,
    string ShelterId,
    Guid? MainPhotoId,
    IReadOnlyCollection<AnimalPhotoDto> Photos,
    IReadOnlyCollection<AnimalEventDto> Events
)
{
    public static AnimalDto FromDomain(Animal a, IBlobStorageService blobStorageService)
    {
        return new AnimalDto(
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
            a.IsActive,
            a.ShelterId,
            a.MainPhotoId,
            a.Photos.Select(p => AnimalPhotoDto.FromDomain(p, blobStorageService)).ToList(),
            a.Events.Select(AnimalEventDto.FromDomain).ToList()
        );
    }
}