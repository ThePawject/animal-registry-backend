using JetBrains.Annotations;

namespace AnimalRegistry.Shared.MediatorPattern;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}