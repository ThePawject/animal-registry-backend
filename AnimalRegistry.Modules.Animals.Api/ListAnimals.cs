using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared.FastEndpoints;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class ListAnimals(IMediator mediator) : Endpoint<EmptyRequest, IEnumerable<AnimalDto>>
{
    public override void Configure()
    {
        Get(ListAnimalsRequest.Route);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ListAnimalsQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}
