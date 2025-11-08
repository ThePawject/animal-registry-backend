using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class CreateAnimalCommand(
    string signature,
    string transponderCode,
    string name,
    string color,
    int dictItemSpeciesId,
    int dictItemSexId,
    DateTimeOffset birthDate)
    : IRequest<CreateAnimalCommandResponse>
{
    public string Signature { get; } = signature;
    public string TransponderCode { get; } = transponderCode;
    public string Name { get; } = name;
    public string Color { get; } = color;
    public int DictItemSpeciesId { get; } = dictItemSpeciesId;
    public int DictItemSexId { get; } = dictItemSexId;
    public DateTimeOffset BirthDate { get; } = birthDate;
}

public sealed class CreateAnimalCommandResponse;