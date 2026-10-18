using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginWriter(
    IShelterFeatureToggles toggles,
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

        var origin = AnimalOrigin.Create(data, DateTimeOffset.UtcNow);
        var municipalities = await municipalityRepository.GetByIdsAsync(
            [origin.FoundMunicipalityId, origin.ContractorMunicipalityId],
            animal.ShelterId,
            cancellationToken);

        animal.SetOrigin(origin, municipalities);

        return Result.Success();
    }
}
