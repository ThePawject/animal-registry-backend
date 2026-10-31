using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record ListExpiringVaccinationsQuery(
    VaccinationValidityFilter Validity,
    int ExpiresInDays,
    bool IncludeExpired,
    bool InShelterOnly,
    int Page,
    int PageSize)
    : IRequest<Result<PagedResult<VaccinationWithAnimalDto>>>;
