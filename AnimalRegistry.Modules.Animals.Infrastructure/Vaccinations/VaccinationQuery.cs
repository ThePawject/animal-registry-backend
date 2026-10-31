using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Vaccinations;

internal sealed class VaccinationQuery(AnimalsDbContext context) : IVaccinationQuery
{
    public async Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetExpiringAsync(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset validBefore,
        bool inShelterOnly,
        CancellationToken cancellationToken = default)
    {
        var rows = await CurrentVaccinations(shelterId, validFrom, validBefore, inShelterOnly)
            .ToListAsync(cancellationToken);

        return rows.Select(ToInfo).ToList();
    }

    public async Task<PagedResult<VaccinationWithAnimalInfo>> ListAsync(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset? validBefore,
        bool inShelterOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = CurrentVaccinations(shelterId, validFrom, validBefore, inShelterOnly);

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VaccinationWithAnimalInfo>(rows.Select(ToInfo).ToList(), totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<VaccinationWithAnimalInfo>> GetAdministeredBetweenAsync(
        string shelterId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var vaccinations = context.AnimalVaccinations
            .AsNoTracking()
            .Where(v => v.ShelterId == shelterId && v.AdministeredOn >= from && v.AdministeredOn < to);
        var animals = context.Animals.AsNoTracking().Where(a => a.ShelterId == shelterId);

        var rows = await JoinAnimals(vaccinations, animals)
            .OrderBy(x => x.Vaccination.AdministeredOn)
            .ThenBy(x => x.Signature)
            .ToListAsync(cancellationToken);

        return rows.Select(ToInfo).ToList();
    }

    private IQueryable<VaccinationRow> CurrentVaccinations(
        string shelterId,
        DateTimeOffset? validFrom,
        DateTimeOffset? validBefore,
        bool inShelterOnly)
    {
        var vaccinations = context.AnimalVaccinations
            .AsNoTracking()
            .Where(v => v.ShelterId == shelterId)
            .Where(v => !context.AnimalVaccinations.Any(r => r.ReplacesVaccinationId == v.Id));

        if (validBefore is { } before)
        {
            vaccinations = vaccinations.Where(v => v.ValidUntil != null && v.ValidUntil < before);
        }

        if (validFrom is { } from)
        {
            vaccinations = vaccinations.Where(v => v.ValidUntil >= from);
        }

        var animals = context.Animals.AsNoTracking().Where(a => a.ShelterId == shelterId);
        if (inShelterOnly)
        {
            animals = animals.Where(a => a.IsInShelter);
        }

        return JoinAnimals(vaccinations, animals)
            .OrderBy(x => x.Vaccination.ValidUntil == null)
            .ThenBy(x => x.Vaccination.ValidUntil)
            .ThenBy(x => x.Signature)
            .ThenBy(x => x.Vaccination.Id);
    }

    private IQueryable<VaccinationRow> JoinAnimals(IQueryable<AnimalVaccination> vaccinations,
        IQueryable<Animal> animals)
    {
        return vaccinations.Join(
            animals,
            v => v.AnimalId,
            a => a.Id,
            (v, a) => new VaccinationRow
            {
                Vaccination = v,
                Name = a.Name,
                Signature = a.Signature,
                Species = a.Species,
                IsInShelter = a.IsInShelter,
                ReplacedByVaccinationId = context.AnimalVaccinations
                    .Where(r => r.ReplacesVaccinationId == v.Id)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefault(),
            });
    }

    private static VaccinationWithAnimalInfo ToInfo(VaccinationRow row)
    {
        return new VaccinationWithAnimalInfo(row.Vaccination, row.Name, row.Signature.Value, row.Species,
            row.IsInShelter, row.ReplacedByVaccinationId);
    }

    private sealed class VaccinationRow
    {
        public required AnimalVaccination Vaccination { get; init; }
        public required string? Name { get; init; }
        public required AnimalSignature Signature { get; init; }
        public required AnimalSpecies Species { get; init; }
        public required bool IsInShelter { get; init; }
        public required Guid? ReplacedByVaccinationId { get; init; }
    }
}
