using AnimalRegistry.Shared.Pagination;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public interface IVaccinationQuery
{
    Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetExpiringAsync(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset validBefore,
        bool inShelterOnly,
        CancellationToken cancellationToken = default);

    Task<PagedResult<VaccinationWithAnimalInfo>> ListAsync(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset? validBefore,
        bool inShelterOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetAdministeredBetweenAsync(
        string shelterId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
