namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class CreateMunicipalityRequest
{
    public const string Route = "/municipalities";

    public string Name { get; init; } = null!;
}
