using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared;

public interface IModule
{
    string Name { get; }
    string Description { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}