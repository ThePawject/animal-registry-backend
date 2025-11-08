namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimalRequest
{
    public required string Signature { get; init; }
    public required string TransponderCode { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public int DictItemSpeciesId { get; init; }
    public int DictItemSexId { get; init; }
    public DateTimeOffset BirthDate { get; init; }
}