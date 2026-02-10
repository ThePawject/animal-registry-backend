using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class CreateAnimalHealth(IMediator mediator) : Endpoint<CreateAnimalHealthRequest>
{
    public override void Configure()
    {
        Post(CreateAnimalHealthRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Adds a new health record to an animal.";
            s.Description = "Adds a new health record to an animal.";
        });
    }

    public override async Task HandleAsync(CreateAnimalHealthRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAnimalHealthCommand(
            req.AnimalId,
            req.OccurredOn,
            req.Description
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}