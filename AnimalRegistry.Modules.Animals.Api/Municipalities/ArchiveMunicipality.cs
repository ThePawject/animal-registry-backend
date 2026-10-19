using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class ArchiveMunicipality(IMediator mediator)
    : Endpoint<ArchiveMunicipalityRequest, MunicipalityDto>
{
    public override void Configure()
    {
        Delete(ArchiveMunicipalityRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Roles(ShelterRoles.Admin);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
    }

    public override async Task HandleAsync(ArchiveMunicipalityRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new SetMunicipalityArchivedCommand(req.Id, true), ct);
        await this.SendResultAsync(result, ct);
    }
}
