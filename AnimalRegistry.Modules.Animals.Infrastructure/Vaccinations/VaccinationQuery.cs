using AnimalRegistry.Modules.Animals.Application.Vaccinations;
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
        var vaccinations = context.AnimalVaccinations
            .AsNoTracking()
            .Where(v => v.ShelterId == shelterId && v.ValidUntil != null && v.ValidUntil < validBefore);

        if (validFrom is { } from)
        {
            vaccinations = vaccinations.Where(v => v.ValidUntil >= from);
        }

        var animals = context.Animals.AsNoTracking().Where(a => a.ShelterId == shelterId);
        if (inShelterOnly)
        {
            animals = animals.Where(a => a.IsInShelter);
        }

        var rows = await vaccinations
            .Join(animals, v => v.AnimalId, a => a.Id, (v, a) => new { Vaccination = v, a.Name, a.Signature, a.Species, a.IsInShelter })
            .OrderBy(x => x.Vaccination.ValidUntil)
            .ThenBy(x => x.Signature)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new VaccinationWithAnimalInfo(x.Vaccination, x.Name, x.Signature.Value, x.Species,
                x.IsInShelter))
            .ToList();
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

        var rows = await vaccinations
            .Join(animals, v => v.AnimalId, a => a.Id, (v, a) => new { Vaccination = v, a.Name, a.Signature, a.Species, a.IsInShelter })
            .OrderBy(x => x.Vaccination.AdministeredOn)
            .ThenBy(x => x.Signature)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new VaccinationWithAnimalInfo(x.Vaccination, x.Name, x.Signature.Value, x.Species,
                x.IsInShelter))
            .ToList();
    }
}
