using AnimalRegistry.Shared.DDD;
using AnimalRegistry.Shared.Outbox.Domain.Rules;

namespace AnimalRegistry.Shared.Outbox.Domain;

public class OutboxMessage : Entity, IAggregateRoot
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        string messageType,
        string messageData,
        string moduleName)
    {
        MessageType = messageType;
        MessageData = messageData;
        ModuleName = moduleName;
        Status = OutboxMessageStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public string MessageType { get; private set; } = default!;
    public string MessageData { get; private set; } = default!;
    public OutboxMessageStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? Error { get; private set; }
    public string ModuleName { get; private set; } = default!;

    public static OutboxMessage Create(
        string messageType,
        string messageData,
        string moduleName)
    {
        CheckRule(new MessageTypeMustNotBeEmptyRule(messageType));
        CheckRule(new MessageDataMustNotBeEmptyRule(messageData));
        CheckRule(new ModuleNameMustNotBeEmptyRule(moduleName));

        return new OutboxMessage(messageType, messageData, moduleName);
    }

    public void MarkAsProcessing()
    {
        Status = OutboxMessageStatus.Processing;
    }

    public void MarkAsProcessed()
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error, TimeSpan nextRetryDelay)
    {
        CheckRule(new ErrorMessageMustNotBeEmptyRule(error));
        CheckRule(new RetryDelayMustNotBeNegativeRule(nextRetryDelay));

        RetryCount++;
        Error = error;
        NextRetryAt = DateTime.UtcNow.Add(nextRetryDelay);
        Status = OutboxMessageStatus.Failed;
    }

    public void ResetForRetry()
    {
        Status = OutboxMessageStatus.Pending;
        NextRetryAt = null;
    }

    public bool CanRetry(int maxRetryCount)
    {
        return RetryCount < maxRetryCount &&
               (NextRetryAt == null || NextRetryAt <= DateTime.UtcNow);
    }
}