using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared.Pagination;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

public sealed record ListExpiringVaccinationsRequest : PaginationRequest
{
    public const string Route = "/vaccinations";

    [QueryParam] public VaccinationValidityFilter Validity { get; init; } = VaccinationValidityFilter.Expiring;

    [QueryParam] public int ExpiresInDays { get; init; } = 30;

    [QueryParam] public bool IncludeExpired { get; init; }

    [QueryParam] public bool InShelterOnly { get; init; } = true;
}
