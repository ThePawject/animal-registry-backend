using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Animals.Application.Origin;

internal sealed class AnimalOriginReader(
    IShelterFeatureToggles toggles,
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

        var municipalityIds = animals
            .Where(a => a.Origin is not null)
            .SelectMany(a => new[] { a.Origin!.FoundMunicipalityId, a.Origin.ContractorMunicipalityId });

        var municipalities =
            await municipalityRepository.GetByIdsAsync(municipalityIds, currentUser.ShelterId, cancellationToken);

        return new AnimalOriginLookup(municipalities.ToDictionary(m => m.Id, m => m.Name));
    }
}
