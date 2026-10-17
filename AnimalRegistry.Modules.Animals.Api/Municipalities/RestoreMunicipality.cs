using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class RestoreMunicipality(IMediator mediator) : EndpointWithoutRequest<MunicipalityDto>
{
    public override void Configure()
    {
        Post(RestoreMunicipalityRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Roles(ShelterRoles.Admin);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await mediator.Send(new SetMunicipalityArchivedCommand(id, false), ct);
        await this.SendResultAsync(result, ct);
    }
}
