namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalRequest
{
    public const string Route = "/animals/{id:guid}";
    public Guid Id { get; set; }
}
