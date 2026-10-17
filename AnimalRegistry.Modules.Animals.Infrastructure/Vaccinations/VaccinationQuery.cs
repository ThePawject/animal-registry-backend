using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
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

        var rows = await JoinAnimals(vaccinations, animals)
            .OrderBy(x => x.Vaccination.ValidUntil)
            .ThenBy(x => x.Signature)
            .ToListAsync(cancellationToken);

        return rows.Select(ToInfo).ToList();
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

    private static IQueryable<VaccinationRow> JoinAnimals(IQueryable<AnimalVaccination> vaccinations,
        IQueryable<Animal> animals)
    {
        return vaccinations.Join(
            animals,
            v => v.AnimalId,
            a => a.Id,
            (v, a) => new VaccinationRow
            {
                Vaccination = v, Name = a.Name, Signature = a.Signature, Species = a.Species,
                IsInShelter = a.IsInShelter,
            });
    }

    private static VaccinationWithAnimalInfo ToInfo(VaccinationRow row)
    {
        return new VaccinationWithAnimalInfo(row.Vaccination, row.Name, row.Signature.Value, row.Species,
            row.IsInShelter);
    }

    private sealed class VaccinationRow
    {
        public required AnimalVaccination Vaccination { get; init; }
        public required string? Name { get; init; }
        public required AnimalSignature Signature { get; init; }
        public required AnimalSpecies Species { get; init; }
        public required bool IsInShelter { get; init; }
    }
}
