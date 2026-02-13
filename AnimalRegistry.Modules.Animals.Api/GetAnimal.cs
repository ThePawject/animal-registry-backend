using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class GetAnimal(IMediator mediator) : Endpoint<GetAnimalRequest, AnimalDto>
{
    public override void Configure()
    {
        Get(GetAnimalRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
    }

    public override async Task HandleAsync(GetAnimalRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAnimalQuery(req.Id), ct);
        await this.SendResultAsync(result, ct);
    }
}