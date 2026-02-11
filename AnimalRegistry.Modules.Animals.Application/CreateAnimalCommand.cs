using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class CreateAnimalCommand(
    AnimalSignature signature,
    string transponderCode,
    string name,
    string color,
    AnimalSpecies species,
    AnimalSex sex,
    DateTimeOffset birthDate,
    List<PhotoUploadInfo>? photos = null,
    int? mainPhotoIndex = null)
    : IRequest<Result<CreateAnimalCommandResponse>>
{
    public AnimalSignature Signature { get; } = signature;
    public string TransponderCode { get; } = transponderCode;
    public string Name { get; } = name;
    public string Color { get; } = color;
    public AnimalSpecies Species { get; } = species;
    public AnimalSex Sex { get; } = sex;
    public DateTimeOffset BirthDate { get; } = birthDate;
    public List<PhotoUploadInfo> Photos { get; } = photos ?? [];
    public int? MainPhotoIndex { get; } = mainPhotoIndex;
}

public sealed record PhotoUploadInfo(string FileName, Stream Content, string ContentType);

public record CreateAnimalCommandResponse(Guid AnimalId);