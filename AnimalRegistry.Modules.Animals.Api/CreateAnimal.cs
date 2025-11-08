using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class CreateAnimal(IMediator mediator) : Endpoint<CreateAnimalRequest>
{
    public override void Configure()
    {
        Post("/animals");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAnimalRequest req, CancellationToken ct)
    {
        await mediator.Send(new CreateAnimalCommand(
            req.Signature,
            req.TransponderCode,
            req.Name,
            req.Color,
            req.DictItemSpeciesId,
            req.DictItemSexId,
            req.BirthDate
        ), ct);
    }
}