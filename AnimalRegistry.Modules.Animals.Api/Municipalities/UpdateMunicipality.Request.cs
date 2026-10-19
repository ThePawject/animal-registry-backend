namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class UpdateMunicipalityRequest
{
    public const string Route = "/municipalities/{id}";

    public Guid Id { get; init; }
    public string Name { get; init; } = null!;

    public static string BuildRoute(Guid id)
    {
        return Route.Replace("{id}", id.ToString());
    }
}
