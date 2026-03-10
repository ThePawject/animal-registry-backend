using AnimalRegistry.Shared.MediatorPattern;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.DDD;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = domainEvent.GetType();
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler == null)
                {
                    continue;
                }

                var handleMethod = handlerType.GetMethod("Handle");
                if (handleMethod != null)
                {
                    await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                }
            }
        }
    }
}