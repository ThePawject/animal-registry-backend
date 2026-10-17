using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application;

public interface IAnimalsUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
