namespace AnimalRegistry.Shared.ShelterFeatures;

internal sealed class DisabledShelterFeatureToggles : IShelterFeatureToggles
{
    private static readonly IReadOnlySet<string> None = new HashSet<string>();

    public Task<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<IReadOnlySet<string>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(None);
    }
}
