using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginWriter(
    IShelterFeatureToggles toggles,
    IAnimalOriginRepository originRepository,
    IMunicipalityRepository municipalityRepository) : IAnimalOriginWriter
{
    public async Task<Result> ApplyAsync(Animal animal, AnimalOriginData? data,
        CancellationToken cancellationToken = default)
    {
        if (!await toggles.IsEnabledAsync(AnimalOriginFeature.FeatureKey, cancellationToken))
        {
            return Result.Success();
        }

        if (data is null)
        {
            return Result.ValidationError("Animal origin is required when the origin module is enabled.");
        }

        var municipalities = await municipalityRepository.GetByIdsAsync(
            [data.FoundMunicipalityId, data.ContractorMunicipalityId],
            animal.ShelterId,
            cancellationToken);
        var origin = await originRepository.GetByAnimalIdAsync(animal.Id, animal.ShelterId, cancellationToken);
        var startOfNextDay = OriginReportPeriod.StartOfNextDay(DateTimeOffset.UtcNow);

        try
        {
            if (origin is null)
            {
                originRepository.Add(AnimalOrigin.Create(animal.Id, animal.ShelterId, data, municipalities,
                    startOfNextDay));
            }
            else
            {
                origin.Update(data, municipalities, startOfNextDay);
            }
        }
        catch (BusinessRuleValidationException ex)
        {
            return Result.ValidationError(ex.Message);
        }

        return Result.Success();
    }
}
