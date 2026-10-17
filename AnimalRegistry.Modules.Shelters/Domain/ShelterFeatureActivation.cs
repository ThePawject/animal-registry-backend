namespace AnimalRegistry.Modules.Shelters.Domain;

internal sealed class ShelterFeatureActivation
{
    private ShelterFeatureActivation()
    {
    }

    private ShelterFeatureActivation(string shelterId, string featureKey, string enabledBy)
    {
        ShelterId = shelterId;
        FeatureKey = featureKey;
        EnabledBy = enabledBy;
        EnabledOn = DateTimeOffset.UtcNow;
    }

    public string ShelterId { get; private set; } = null!;
    public string FeatureKey { get; private set; } = null!;
    public string EnabledBy { get; private set; } = null!;
    public DateTimeOffset EnabledOn { get; private set; }

    public static ShelterFeatureActivation Create(string shelterId, string featureKey, string enabledBy)
    {
        return new ShelterFeatureActivation(shelterId, featureKey, enabledBy);
    }
}
