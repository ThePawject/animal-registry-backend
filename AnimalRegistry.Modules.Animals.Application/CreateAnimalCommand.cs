using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class CreateAnimalCommand : IRequest<Result<CreateAnimalCommandResponse>>
{
    public CreateAnimalCommand(
        string signature,
        string transponderCode,
        string name,
        string color,
        AnimalSpecies species,
        AnimalSex sex,
        DateTimeOffset birthDate,
        List<PhotoUploadInfo>? photos = null,
        int? mainPhotoIndex = null)
    {
        Signature = signature;
        TransponderCode = transponderCode;
        Name = name;
        Color = color;
        Species = species;
        Sex = sex;
        BirthDate = birthDate;
        Photos = photos ?? [];
        MainPhotoIndex = mainPhotoIndex;
    }

    public string Signature { get; }
    public string TransponderCode { get; }
    public string Name { get; }
    public string Color { get; }
    public AnimalSpecies Species { get; }
    public AnimalSex Sex { get; }
    public DateTimeOffset BirthDate { get; }
    public List<PhotoUploadInfo> Photos { get; }
    public int? MainPhotoIndex { get; }
}

public sealed record PhotoUploadInfo(string FileName, Stream Content, string ContentType);

public record CreateAnimalCommandResponse(Guid AnimalId);