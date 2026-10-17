using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginReader(
    IShelterFeatureToggles toggles,
    IAnimalOriginRepository originRepository,
    IMunicipalityRepository municipalityRepository,
    ICurrentUser currentUser) : IAnimalOriginReader
{
    public async Task<AnimalOriginLookup?> LoadAsync(IReadOnlyCollection<Animal> animals,
        CancellationToken cancellationToken = default)
    {
        if (!await toggles.IsEnabledAsync(AnimalOriginFeature.FeatureKey, cancellationToken))
        {
            return null;
        }

        var origins = await originRepository.GetByAnimalIdsAsync(animals.Select(a => a.Id), currentUser.ShelterId,
            cancellationToken);

        var municipalityIds = origins
            .SelectMany(o => new[] { o.FoundMunicipalityId, o.ContractorMunicipalityId });

        var municipalities =
            await municipalityRepository.GetByIdsAsync(municipalityIds, currentUser.ShelterId, cancellationToken);

        return new AnimalOriginLookup(
            origins.ToDictionary(o => o.AnimalId),
            municipalities.ToDictionary(m => m.Id, m => m.Name));
    }
}
