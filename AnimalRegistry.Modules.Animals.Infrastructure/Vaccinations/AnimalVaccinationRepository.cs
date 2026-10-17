using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Vaccinations;

internal sealed class AnimalVaccinationRepository(AnimalsDbContext context) : IAnimalVaccinationRepository
{
    public Task<AnimalVaccination?> GetByIdAsync(Guid id, Guid animalId, string shelterId,
        CancellationToken cancellationToken = default)
    {
        return context.AnimalVaccinations
            .FirstOrDefaultAsync(v => v.Id == id && v.AnimalId == animalId && v.ShelterId == shelterId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AnimalVaccination>> ListByAnimalIdAsync(Guid animalId, string shelterId,
        CancellationToken cancellationToken = default)
    {
        return await context.AnimalVaccinations
            .AsNoTracking()
            .Where(v => v.AnimalId == animalId && v.ShelterId == shelterId)
            .OrderByDescending(v => v.AdministeredOn)
            .ThenByDescending(v => v.CreatedOn)
            .ToListAsync(cancellationToken);
    }

    public void Add(AnimalVaccination vaccination)
    {
        context.AnimalVaccinations.Add(vaccination);
    }

    public void Remove(AnimalVaccination vaccination)
    {
        context.AnimalVaccinations.Remove(vaccination);
    }
}
