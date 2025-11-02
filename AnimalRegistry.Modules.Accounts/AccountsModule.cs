using AnimalRegistry.Modules.Accounts.Api;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Modules.Accounts;

public sealed class AccountsModule : IModule
{
    public string Name { get; } = "Accounts";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies =
            [
                typeof(MyEndpoint).Assembly,
            ];
        });

        services.AddMyMediator(typeof(MyEndpoint).Assembly);
    }
}