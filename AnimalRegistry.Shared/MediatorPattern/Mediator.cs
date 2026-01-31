using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.MediatorPattern;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod("Handle");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler for {requestType} does not contain a Handle method.");
        }

        var result = handleMethod.Invoke(handler, [request, cancellationToken]);
        return result as Task<TResponse> ??
               throw new InvalidOperationException($"Handle method returned unexpected result for {requestType}.");
    }
}