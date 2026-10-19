using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class CreateMunicipality(IMediator mediator) : Endpoint<CreateMunicipalityRequest, MunicipalityDto>
{
    public override void Configure()
    {
        Post(CreateMunicipalityRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Roles(ShelterRoles.Admin);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
    }

    public override async Task HandleAsync(CreateMunicipalityRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateMunicipalityCommand(req.Name), ct);
        await this.SendResultAsync(result, ct);
    }
}
