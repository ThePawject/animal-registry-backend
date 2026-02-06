namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalRequest
{
    public const string Route = "/animals/{id}";
    public Guid Id { get; init; }

    public static string BuildRoute(Guid id)
    {
        return Route.Replace("{id}", id.ToString());
    }
}