namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class SetMunicipalityArchivedRequest
{
    public const string ArchiveRoute = "/municipalities/{id}";
    public const string RestoreRoute = "/municipalities/{id}/restore";

    public Guid Id { get; init; }

    public static string BuildArchiveRoute(Guid id)
    {
        return ArchiveRoute.Replace("{id}", id.ToString());
    }

    public static string BuildRestoreRoute(Guid id)
    {
        return RestoreRoute.Replace("{id}", id.ToString());
    }
}
