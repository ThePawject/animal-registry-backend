using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed record ListExpiringVaccinationsQuery(int ExpiresInDays, bool IncludeExpired, bool InShelterOnly)
    : IRequest<Result<IReadOnlyList<VaccinationWithAnimalDto>>>;
