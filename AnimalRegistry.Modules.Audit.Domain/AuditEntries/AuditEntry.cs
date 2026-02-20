using AnimalRegistry.Modules.Audit.Domain.AuditEntries.Rules;
using AnimalRegistry.Shared.DDD;

namespace AnimalRegistry.Modules.Audit.Domain.AuditEntries;

public class AuditEntry : Entity, IAggregateRoot
{
    private AuditEntry()
    {
    }

    private AuditEntry(
        AuditEntryType type,
        string entityType,
        string entityData,
        AuditMetadata metadata,
        TimeSpan? executionTime,
        bool isSuccess,
        string? errorMessage)
    {
        Type = type;
        EntityType = entityType;
        EntityData = entityData;
        Metadata = metadata;
        Timestamp = DateTime.UtcNow;
        ExecutionTime = executionTime;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public AuditEntryType Type { get; private set; }
    public string EntityType { get; private set; } = default!;
    public string EntityData { get; private set; } = default!;
    public AuditMetadata Metadata { get; private set; } = default!;
    public DateTime Timestamp { get; private set; }
    public TimeSpan? ExecutionTime { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static AuditEntry CreateForDomainEvent(
        string entityType,
        string entityData,
        AuditMetadata metadata)
    {
        CheckRule(new EntityTypeMustNotBeEmptyRule(entityType));
        CheckRule(new EntityDataMustNotBeEmptyRule(entityData));

        return new AuditEntry(
            AuditEntryType.DomainEvent,
            entityType,
            entityData,
            metadata,
            null,
            true,
            null);
    }

    public static AuditEntry CreateForCommand(
        string entityType,
        string entityData,
        AuditMetadata metadata,
        TimeSpan executionTime,
        bool isSuccess,
        string? errorMessage = null)
    {
        CheckRule(new EntityTypeMustNotBeEmptyRule(entityType));
        CheckRule(new EntityDataMustNotBeEmptyRule(entityData));
        CheckRule(new ExecutionTimeMustNotBeNegativeRule(executionTime));

        return new AuditEntry(
            AuditEntryType.Command,
            entityType,
            entityData,
            metadata,
            executionTime,
            isSuccess,
            errorMessage);
    }
}