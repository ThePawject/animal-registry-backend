using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

public sealed class ListMunicipalitiesRequest
{
    public const string Route = "/municipalities";

    [QueryParam] public bool IncludeArchived { get; init; }
}
