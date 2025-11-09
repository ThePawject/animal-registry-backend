using AnimalRegistry.Modules.Animals.Domain.Animals;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure.Animals;

public class AnimalRepository : IAnimalRepository
{
    private readonly AnimalsDbContext _context;

    public AnimalRepository(AnimalsDbContext context)
    {
        _context = context;
    }

    public async Task<Animal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Animals.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Animal entity, CancellationToken cancellationToken = default)
    {
        await _context.Animals.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Animal entity)
    {
        _context.Animals.Remove(entity);
        _context.SaveChanges();
    }
}
