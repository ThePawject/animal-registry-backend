using AnimalRegistry.Modules.Shelters.Domain;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Shelters.Application;

internal sealed class ShelterFeatureToggles(
    IShelterFeatureActivationRepository repository,
    ICurrentUser currentUser) : IShelterFeatureToggles
{
    private IReadOnlySet<string>? _enabled;

    public async Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        var enabled = await GetEnabledAsync(cancellationToken);
        return enabled.Contains(featureKey);
    }

    public async Task<IReadOnlySet<string>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        if (_enabled is not null)
        {
            return _enabled;
        }

        if (string.IsNullOrEmpty(currentUser.ShelterId))
        {
            return _enabled = new HashSet<string>();
        }

        var activations = await repository.GetByShelterIdAsync(currentUser.ShelterId, cancellationToken);
        return _enabled = activations.Select(a => a.FeatureKey).ToHashSet();
    }
}
