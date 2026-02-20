namespace AnimalRegistry.Shared.Outbox.Domain;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3,
}