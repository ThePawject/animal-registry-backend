using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AnimalRegistry.Shared;

public interface IModule
{
    string Name { get; }

    IReadOnlyCollection<Assembly> EndpointAssemblies { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    Task MigrateAsync(IServiceProvider serviceProvider);
}