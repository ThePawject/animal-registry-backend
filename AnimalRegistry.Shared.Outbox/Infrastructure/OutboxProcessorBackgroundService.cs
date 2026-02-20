using AnimalRegistry.Shared.Outbox.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Shared.Outbox.Infrastructure;

public class OutboxProcessorBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxSettings> settings,
    ILogger<OutboxProcessorBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxSettings _settings = settings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Outbox Processor Background Service started. Polling interval: {PollingInterval}s",
            _settings.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_settings.PollingInterval, stoppingToken);
        }

        logger.LogInformation("Outbox Processor Background Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

        var pendingMessages = await repository.GetPendingMessagesAsync(
            _settings.BatchSize,
            cancellationToken);

        if (pendingMessages.Any())
        {
            logger.LogInformation("Processing {Count} pending outbox messages", pendingMessages.Count);

            foreach (var message in pendingMessages)
            {
                await processor.ProcessAsync(message, cancellationToken);
            }

            await repository.SaveChangesAsync(cancellationToken);
        }

        var failedMessages = await repository.GetFailedMessagesReadyForRetryAsync(
            _settings.BatchSize,
            _settings.MaxRetryCount,
            cancellationToken);

        if (failedMessages.Any())
        {
            logger.LogInformation("Retrying {Count} failed outbox messages", failedMessages.Count);

            foreach (var message in failedMessages)
            {
                message.ResetForRetry();
                await processor.ProcessAsync(message, cancellationToken);
            }

            await repository.SaveChangesAsync(cancellationToken);
        }
    }
}