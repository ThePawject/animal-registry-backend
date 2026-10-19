using AnimalRegistry.Modules.Shelters.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Shelters.Api;

internal sealed class GetShelterFeatures(IMediator mediator)
    : EndpointWithoutRequest<IReadOnlyList<ShelterFeatureDto>>
{
    public const string Route = "/shelter/features";

    public override void Configure()
    {
        Get(Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Summary(s =>
        {
            s.Summary = "List shelter modules";
            s.Description = "Returns all available modules with information whether they are enabled for the shelter.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetShelterFeaturesQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}
