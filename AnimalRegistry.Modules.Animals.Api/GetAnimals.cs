using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared.FastEndpoints;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimals(IMediator mediator) : Endpoint<GetAnimalsRequest, GetAllAnimalsQueryResponse>
{
    public override void Configure()
    {
        Get(GetAnimalsRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAnimalsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllAnimalsQuery(), ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
