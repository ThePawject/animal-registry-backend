using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.Outbox.Application;
using AnimalRegistry.Shared.Outbox.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AnimalRegistry.Shared.Outbox.Infrastructure;

public class OutboxProcessor(
    IDomainEventDispatcher domainEventDispatcher,
    IOptions<OutboxSettings> settings,
    ILogger<OutboxProcessor> logger)
    : IOutboxProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly OutboxSettings _settings = settings.Value;

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug(
                "Processing outbox message {MessageId} of type {MessageType} from module {ModuleName}",
                message.Id, message.MessageType, message.ModuleName);

            message.MarkAsProcessing();

            var domainEvent = DeserializeDomainEvent(message);

            await domainEventDispatcher.DispatchAsync([domainEvent], cancellationToken);

            message.MarkAsProcessed();

            logger.LogInformation(
                "Successfully processed outbox message {MessageId} of type {MessageType}",
                message.Id, message.MessageType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing outbox message {MessageId} of type {MessageType}. Retry count: {RetryCount}",
                message.Id, message.MessageType, message.RetryCount);

            var nextRetryDelay = _settings.CalculateRetryDelay(message.RetryCount);
            message.MarkAsFailed(ex.Message, nextRetryDelay);
        }
    }

    private static IDomainEvent DeserializeDomainEvent(OutboxMessage message)
    {
        var eventType = Type.GetType(message.MessageType);

        if (eventType == null)
        {
            throw new InvalidOperationException(
                $"Cannot find type {message.MessageType}. Make sure the assembly is loaded.");
        }

        var domainEvent = JsonSerializer.Deserialize(message.MessageData, eventType, JsonOptions);

        if (domainEvent is not IDomainEvent typedEvent)
        {
            throw new InvalidOperationException(
                $"Type {message.MessageType} is not a valid IDomainEvent.");
        }

        return typedEvent;
    }
}