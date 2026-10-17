using AnimalRegistry.Modules.Shelters.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Shelters.Api;

internal sealed class SetShelterFeatureState(IMediator mediator)
    : Endpoint<SetShelterFeatureStateRequest, ShelterFeatureDto>
{
    public override void Configure()
    {
        Put(SetShelterFeatureStateRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Roles(ShelterRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Enable or disable shelter module";
            s.Description = "Requires the Shelter_Admin role.";
        });
    }

    public override async Task HandleAsync(SetShelterFeatureStateRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new SetShelterFeatureStateCommand(req.Key, req.IsEnabled), ct);
        await this.SendResultAsync(result, ct);
    }
}
