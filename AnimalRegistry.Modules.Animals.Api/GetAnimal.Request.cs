namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalRequest
{
    public const string Route = "/animals/{id}";
    public required Guid Id { get; init; }
}
