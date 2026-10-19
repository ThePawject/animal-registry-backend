using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalDto(
    Guid Id,
    string Signature,
    string? TransponderCode,
    string? Name,
    string Color,
    string Breed,
    string DistinguishingMarks,
    AnimalSpecies Species,
    AnimalSex Sex,
    DateTimeOffset? BirthDate,
    DateTimeOffset CreatedOn,
    DateTimeOffset ModifiedOn,
    bool IsInShelter,
    string ShelterId,
    Guid? MainPhotoId,
    IReadOnlyCollection<AnimalPhotoDto> Photos,
    IReadOnlyCollection<AnimalEventDto> Events,
    IReadOnlyCollection<AnimalHealthDto> HealthRecords,
    AnimalOriginDto? Origin = null
)
{
    public static AnimalDto FromDomain(Animal a, AnimalOriginDto? origin = null)
    {
        return new AnimalDto(
            a.Id,
            a.Signature.Value,
            a.TransponderCode,
            a.Name,
            a.Color,
            a.Breed,
            a.DistinguishingMarks,
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
            a.HealthRecords.Select(AnimalHealthDto.FromDomain).ToList(),
            origin
        );
    }
}