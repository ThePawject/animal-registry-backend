using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure.Origin;
using AnimalRegistry.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AnimalRegistry.Modules.Animals.Infrastructure;

internal sealed class AnimalsUnitOfWork(AnimalsDbContext context) : IAnimalsUnitOfWork
{
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (IsDuplicateAnimalOrigin(ex))
        {
            return Result.ValidationError(
                "Origin of this animal was saved by another request. Reload the animal and try again.");
        }
    }

    private static bool IsDuplicateAnimalOrigin(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
               sqlException.Message.Contains(AnimalOriginConfiguration.PrimaryKeyName);
    }
}
