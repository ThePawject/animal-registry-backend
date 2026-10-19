namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class RestoreMunicipalityRequest
{
    public const string Route = "/municipalities/{id}/restore";

    public static string BuildRoute(Guid id)
    {
        return Route.Replace("{id}", id.ToString());
    }
}
