using AnimalRegistry.Shared.MediatorPattern;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.Telemetry;

public static class TracedMediatorExtensions
{
    public static IServiceCollection AddMediatorTracing(this IServiceCollection services)
    {
        var registration = services.LastOrDefault(descriptor => descriptor.ServiceType == typeof(IMediator))
                           ?? throw new InvalidOperationException(
                               "IMediator must be registered before AddMediatorTracing is called.");

        var innerType = registration.ImplementationType
                        ?? throw new InvalidOperationException("IMediator must be registered with an implementation type.");

        services.Remove(registration);
        services.Add(new ServiceDescriptor(innerType, innerType, registration.Lifetime));
        services.Add(new ServiceDescriptor(
            typeof(IMediator),
            provider => new TracedMediator((IMediator)provider.GetRequiredService(innerType)),
            registration.Lifetime));

        return services;
    }
}
