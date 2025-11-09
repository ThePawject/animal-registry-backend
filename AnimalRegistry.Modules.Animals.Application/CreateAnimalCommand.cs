using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

public sealed class CreateAnimalCommand(
    string signature,
    string transponderCode,
    string name,
    string color,
    AnimalSpecies species,
    AnimalSex sex,
    DateTimeOffset birthDate)
    : IRequest<CreateAnimalCommandResponse>
{
    public string Signature { get; } = signature;
    public string TransponderCode { get; } = transponderCode;
    public string Name { get; } = name;
    public string Color { get; } = color;
    public AnimalSpecies Species { get; } = species;
    public AnimalSex Sex { get; } = sex;
    public DateTimeOffset BirthDate { get; } = birthDate;
}

public sealed class CreateAnimalCommandResponse;