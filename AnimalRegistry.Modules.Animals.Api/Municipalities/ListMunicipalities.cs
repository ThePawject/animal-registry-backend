using AnimalRegistry.Modules.Animals.Application.Municipalities;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Municipalities;

internal sealed class ListMunicipalities(IMediator mediator)
    : Endpoint<ListMunicipalitiesRequest, IReadOnlyList<MunicipalityDto>>
{
    public override void Configure()
    {
        Get(ListMunicipalitiesRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalOriginFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "List municipalities";
            s.Description = "Municipalities (contractors) of the animal origin module, alphabetically.";
        });
    }

    public override async Task HandleAsync(ListMunicipalitiesRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ListMunicipalitiesQuery(req.IncludeArchived), ct);
        await this.SendResultAsync(result, ct);
    }
}
