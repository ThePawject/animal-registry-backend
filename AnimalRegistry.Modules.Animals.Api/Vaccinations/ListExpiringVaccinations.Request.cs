using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed class ListExpiringVaccinationsRequest
{
    public const string Route = "/vaccinations";

    [QueryParam] public int ExpiresInDays { get; init; } = 30;

    [QueryParam] public bool IncludeExpired { get; init; }

    [QueryParam] public bool InShelterOnly { get; init; } = true;
}
