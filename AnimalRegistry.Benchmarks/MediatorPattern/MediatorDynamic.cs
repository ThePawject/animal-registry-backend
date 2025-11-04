using Microsoft.Extensions.DependencyInjection;

namespace AnimalRegistry.Benchmarks.MediatorPattern;

public interface IRequest<TResponse>
{
}

public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public class MediatorDynamic(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);
        return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }
}