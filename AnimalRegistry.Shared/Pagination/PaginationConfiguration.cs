using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.Pagination;

public static class PaginationConfiguration
{
    public static IServiceCollection AddPagination(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaginationSettings>(configuration.GetSection("Pagination"));
        return services;
    }
}
