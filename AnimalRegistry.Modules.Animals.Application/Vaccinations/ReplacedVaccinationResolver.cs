using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class ReplacedVaccinationResolver(IAnimalVaccinationRepository vaccinationRepository)
{
    public async Task<Result<ReplacedVaccination>> ResolveAsync(
        Guid? replacesVaccinationId,
        Guid animalId,
        Guid? replacingVaccinationId,
        string shelterId,
        CancellationToken cancellationToken)
    {
        if (replacesVaccinationId is not { } replacedId)
        {
            return Result<ReplacedVaccination>.Success(ReplacedVaccination.None);
        }

        var replaced = await vaccinationRepository.GetByIdAsync(replacedId, animalId, shelterId, cancellationToken);
        if (replaced is null)
        {
            return Result<ReplacedVaccination>.ValidationError("Replaced vaccination not found.");
        }

        var replacing = await vaccinationRepository.FindReplacingAsync(replaced.Id, shelterId, cancellationToken);
        var isReplacedElsewhere = replacing is not null && replacing.Id != replacingVaccinationId;

        return Result<ReplacedVaccination>.Success(new ReplacedVaccination(replaced, isReplacedElsewhere));
    }
}
