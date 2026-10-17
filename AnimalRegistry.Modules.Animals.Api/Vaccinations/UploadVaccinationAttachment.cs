using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class UploadVaccinationAttachment(IMediator mediator)
    : Endpoint<UploadVaccinationAttachmentRequest, AnimalVaccinationDto>
{
    public override void Configure()
    {
        Put(UploadVaccinationAttachmentRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        AllowFormData();
        AllowFileUploads();
        Options(b => b.RequireShelterFeature(AnimalVaccinationFeature.FeatureKey));
        Summary(s =>
        {
            s.Summary = "Upload vaccination attachment";
            s.Description = "Sets the attachment of a vaccination, replacing the previous one.";
        });
    }

    public override async Task HandleAsync(UploadVaccinationAttachmentRequest req, CancellationToken ct)
    {
        await using var content = req.File!.OpenReadStream();
        var file = new DocumentUploadInfo(req.File.FileName, content, req.File.ContentType);
        var result = await mediator.Send(new SetVaccinationAttachmentCommand(req.AnimalId, req.VaccinationId, file),
            ct);
        await this.SendResultAsync(result, ct);
    }
}
