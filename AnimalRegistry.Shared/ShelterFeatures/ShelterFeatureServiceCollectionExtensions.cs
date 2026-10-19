using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimalRegistry.Shared.ShelterFeatures;

public static class ShelterFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddShelterFeature<TFeature>(this IServiceCollection services)
        where TFeature : class, IShelterFeature
    {
        services.AddSingleton<IShelterFeature, TFeature>();
        services.TryAddScoped<IShelterFeatureToggles, DisabledShelterFeatureToggles>();
        return services;
    }

    public static IServiceCollection AddShelterFeatureToggles<TToggles>(this IServiceCollection services)
        where TToggles : class, IShelterFeatureToggles
    {
        services.Replace(ServiceDescriptor.Scoped<IShelterFeatureToggles, TToggles>());
        return services;
    }
}
