using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Shared.MediatorPattern;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = serviceProvider.GetRequiredService(handlerType);

        return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }
}