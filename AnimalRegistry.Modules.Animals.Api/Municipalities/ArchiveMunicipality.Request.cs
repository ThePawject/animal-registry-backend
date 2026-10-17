namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class ArchiveMunicipalityRequest
{
    public const string Route = "/municipalities/{id}";

    public Guid Id { get; init; }

    public static string BuildRoute(Guid id)
    {
        return Route.Replace("{id}", id.ToString());
    }
}
