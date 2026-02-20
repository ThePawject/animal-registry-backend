using AnimalRegistry.Shared.Outbox.Application;
using AnimalRegistry.Shared.Outbox.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.Outbox.Extensions;

public static class OutboxExtensions
{
    /// <summary>
    ///     Registers Outbox pattern services including background processor
    /// </summary>
    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxSettings>(
            configuration.GetSection("Outbox"));

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }

    /// <summary>
    ///     Adds OutboxInterceptor to DbContext to automatically capture domain events
    /// </summary>
    public static DbContextOptionsBuilder AddOutboxInterceptor(
        this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new OutboxInterceptor());
        return optionsBuilder;
    }

    /// <summary>
    ///     Extension method to apply OutboxMessage configuration to DbContext
    ///     Call this in OnModelCreating if you want to manually configure
    /// </summary>
    public static ModelBuilder ApplyOutboxConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }
}