using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class DeleteAnimal(IMediator mediator) : Endpoint<DeleteAnimalRequest>
{
    public override void Configure()
    {
        Delete(DeleteAnimalRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
    }

    public override async Task HandleAsync(DeleteAnimalRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAnimalCommand(req.Id), ct);
        await this.SendResultAsync(result, ct);
    }
}