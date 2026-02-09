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
);
