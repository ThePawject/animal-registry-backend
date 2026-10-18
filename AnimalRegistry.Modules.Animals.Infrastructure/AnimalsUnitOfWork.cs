using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Infrastructure.Origin;
using AnimalRegistry.Modules.Animals.Infrastructure.Vaccinations;
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
        catch (DbUpdateException ex) when (IsDuplicateVaccinationReplacement(ex))
        {
            return Result.ValidationError("This vaccination has already been replaced by another one.");
        }
    }

    private static bool IsDuplicateAnimalOrigin(DbUpdateException exception)
    {
        return IsDuplicateIndex(exception, AnimalOriginConfiguration.PrimaryKeyName);
    }

    private static bool IsDuplicateVaccinationReplacement(DbUpdateException exception)
    {
        return IsDuplicateIndex(exception, AnimalVaccinationConfiguration.ReplacesVaccinationIdIndexName);
    }

    private static bool IsDuplicateIndex(DbUpdateException exception, string indexName)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
               sqlException.Message.Contains(indexName);
    }
}
