using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class DeleteVaccinationAttachment(IMediator mediator) : Endpoint<DeleteVaccinationAttachmentRequest>
{
    public override void Configure()
    {
        Delete(DeleteVaccinationAttachmentRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Delete vaccination attachment";
            s.Description = "Removes the attachment of a vaccination.";
        });
    }

    public override async Task HandleAsync(DeleteVaccinationAttachmentRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteVaccinationAttachmentCommand(req.AnimalId, req.VaccinationId),
            ct);
        await this.SendResultAsync(result, ct);
    }
}
