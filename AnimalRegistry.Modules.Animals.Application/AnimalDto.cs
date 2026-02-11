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
    bool IsInShelter,
    string ShelterId,
    Guid? MainPhotoId,
    IReadOnlyCollection<AnimalPhotoDto> Photos,
    IReadOnlyCollection<AnimalEventDto> Events,
    IReadOnlyCollection<AnimalHealthDto> HealthRecords
)
{
    public static AnimalDto FromDomain(Animal a)
    {
        return new AnimalDto(
            a.Id,
            a.Signature.Value,
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
            a.Photos.Select(p => new AnimalPhotoDto(
                p.Id,
                p.Url ?? string.Empty,
                p.FileName,
                p.UploadedOn
            )).ToList(),
            a.Events.Select(AnimalEventDto.FromDomain).ToList(),
            a.HealthRecords.Select(AnimalHealthDto.FromDomain).ToList()
        );
    }
}