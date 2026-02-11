using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class UpdateAnimalCommand(
    Guid id,
    AnimalSignature signature,
    string transponderCode,
    string name,
    string color,
    AnimalSpecies species,
    AnimalSex sex,
    DateTimeOffset birthDate,
    List<PhotoUploadInfo> newPhotos,
    List<Guid> existingPhotoIds,
    Guid? mainPhotoId,
    int? mainPhotoIndex)
    : IRequest<Result<UpdateAnimalCommandResponse>>
{
    public Guid Id { get; } = id;
    public AnimalSignature Signature { get; } = signature;
    public string TransponderCode { get; } = transponderCode;
    public string Name { get; } = name;
    public string Color { get; } = color;
    public AnimalSpecies Species { get; } = species;
    public AnimalSex Sex { get; } = sex;
    public DateTimeOffset BirthDate { get; } = birthDate;
    public List<PhotoUploadInfo> NewPhotos { get; } = newPhotos;
    public List<Guid> ExistingPhotoIds { get; } = existingPhotoIds;
    public Guid? MainPhotoId { get; } = mainPhotoId;
    public int? MainPhotoIndex { get; } = mainPhotoIndex;
}

public record UpdateAnimalCommandResponse(Guid AnimalId);