using AnimalRegistry.Shared.ShelterFeatures;

namespace AnimalRegistry.Modules.Shelters.Application;

public sealed record ShelterFeatureDto(string Key, string Name, string Description, bool IsEnabled)
{
    internal static ShelterFeatureDto From(IShelterFeature feature, bool isEnabled)
    {
        return new ShelterFeatureDto(feature.Key, feature.Name, feature.Description, isEnabled);
    }
}
