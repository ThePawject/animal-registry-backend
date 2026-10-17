namespace AnimalRegistry.Shared.ShelterFeatures;

public interface IShelterFeatureToggles
{
    Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetEnabledAsync(CancellationToken cancellationToken = default);
}
