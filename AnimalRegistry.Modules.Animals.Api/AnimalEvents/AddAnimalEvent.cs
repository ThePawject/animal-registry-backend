using AnimalRegistry.Modules.Animals.Application.AnimalEvents;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalEvents;

public sealed class AddAnimalEvent(IMediator mediator) : Endpoint<AddAnimalEventRequest>
{
    public override void Configure()
    {
        Post(AddAnimalEventRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "Adds a new event to an animal.";
            s.Description = "Adds a new event to an animal.";
        });
    }

    public override async Task HandleAsync(AddAnimalEventRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new AddAnimalEventCommand(
            req.AnimalId,
            req.Type,
            req.OccurredOn,
            req.Description,
            req.PerformedBy
        ), ct);


        await this.SendResultAsync(result, ct);
    }
}