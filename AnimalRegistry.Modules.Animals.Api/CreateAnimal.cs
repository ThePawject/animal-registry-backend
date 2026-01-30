using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared.FastEndpoints;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimal(IMediator mediator) : Endpoint<CreateAnimalRequest, CreateAnimalCommandResponse>
{
    public override void Configure()
    {
        Post(CreateAnimalRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAnimalRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAnimalCommand(
            req.Signature,
            req.TransponderCode,
            req.Name,
            req.Color,
            req.Species,
            req.Sex,
            req.BirthDate
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}