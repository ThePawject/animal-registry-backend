using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Shared.DDD;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOn { get; }
}
