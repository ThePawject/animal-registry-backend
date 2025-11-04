namespace AnimalRegistry.Benchmarks.MediatorPattern;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}