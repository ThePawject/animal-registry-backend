using AnimalRegistry.Modules.Animals.Application.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class CreateAnimalEvent(IMediator mediator) : Endpoint<CreateAnimalEventRequest>
{
    public override void Configure()
    {
        Post(CreateAnimalEventRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Adds a new event to an animal.";
            s.Description = "Adds a new event to an animal.";
        });
    }

    public override async Task HandleAsync(CreateAnimalEventRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAnimalEventCommand(
            req.AnimalId,
            req.Type,
            req.OccurredOn,
            req.Description
        ), ct);


        await this.SendResultAsync(result, ct);
    }
}