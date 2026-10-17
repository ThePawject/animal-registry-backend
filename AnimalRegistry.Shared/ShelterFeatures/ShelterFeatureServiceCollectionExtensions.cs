using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.ShelterFeatures;

public static class ShelterFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddShelterFeature<TFeature>(this IServiceCollection services)
        where TFeature : class, IShelterFeature
    {
        services.AddSingleton<IShelterFeature, TFeature>();
        return services;
    }
}
