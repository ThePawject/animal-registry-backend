using AnimalRegistry.Modules.Animals.Domain.Animals;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimalRequest
{
    public const string Route = "/animals";
    public required string Signature { get; init; }
    public required string TransponderCode { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public AnimalSpecies Species { get; init; }
    public AnimalSex Sex { get; init; }
    public DateTimeOffset BirthDate { get; init; }
}