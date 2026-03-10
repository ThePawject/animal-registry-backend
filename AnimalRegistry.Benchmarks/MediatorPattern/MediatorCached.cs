using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace AnimalRegistry.Benchmarks.MediatorPattern;

internal abstract class RequestHandlerWrapper
{
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.Handle((TRequest)request, cancellationToken);
    }
}

public class MediatorCached(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> _requestHandlers = new();

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        if (!_requestHandlers.TryGetValue(requestType, out var wrapper))
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
            wrapper = (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
            _requestHandlers.TryAdd(requestType, wrapper);
        }

        var result = await wrapper.Handle(request, serviceProvider, cancellationToken);
        return (TResponse)result!;
    }
}