using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimal(IMediator mediator) : Endpoint<CreateAnimalRequest>
{
    public override void Configure()
    {
        Post(CreateAnimalRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAnimalRequest req, CancellationToken ct)
    {
        await mediator.Send(new CreateAnimalCommand(
            req.Signature,
            req.TransponderCode,
            req.Name,
            req.Color,
            req.Species,
            req.Sex,
            req.BirthDate
        ), ct);
    }
}