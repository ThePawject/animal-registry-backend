using System.Diagnostics;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Shared.Telemetry;

public sealed class TracedMediator(IMediator inner) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var operationName = request.GetType().Name;

        using var activity = AnimalRegistryTelemetry.Source.StartActivity($"Operation {operationName}", ActivityKind.Internal);
        activity?.SetTag("operation.name", operationName);

        try
        {
            var result = await inner.Send(request, cancellationToken);

            if (result is Result { IsFailure: true } failed)
            {
                activity?.SetTag("operation.status", failed.Status.ToString());
                activity?.SetStatus(ActivityStatusCode.Error, failed.Error);
            }
            else
            {
                activity?.SetTag("operation.status", "Ok");
            }

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("operation.status", "Exception");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.type", ex.GetType().FullName },
                { "exception.message", ex.Message },
            }));
            throw;
        }
    }
}
