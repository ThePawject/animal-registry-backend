namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalRequest
{
    public const string Route = "/animals/{id}";
    public Guid Id { get; set; }

    public static string BuildRoute(Guid id) => Route.Replace("{id}", id.ToString());
}
