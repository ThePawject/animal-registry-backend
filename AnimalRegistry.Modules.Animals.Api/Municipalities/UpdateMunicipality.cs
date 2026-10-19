using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class UpdateMunicipality(IMediator mediator) : Endpoint<UpdateMunicipalityRequest, MunicipalityDto>
{
    public override void Configure()
    {
        Put(UpdateMunicipalityRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Roles(ShelterRoles.Admin);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
    }

    public override async Task HandleAsync(UpdateMunicipalityRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateMunicipalityCommand(req.Id, req.Name), ct);
        await this.SendResultAsync(result, ct);
    }
}
