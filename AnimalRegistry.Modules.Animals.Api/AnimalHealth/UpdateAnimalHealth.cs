using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class UpdateAnimalHealth(IMediator mediator) : Endpoint<UpdateAnimalHealthRequest>
{
    public override void Configure()
    {
        Put(UpdateAnimalHealthRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        AllowFormData();
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Updates an existing health record for an animal.";
            s.Description = "Updates an existing health record for an animal with optional document replacement or deletion.";
        });
    }

    public override async Task HandleAsync(UpdateAnimalHealthRequest req, CancellationToken ct)
    {
        var documentFile = req.DocumentFile != null
            ? new DocumentUploadInfo(
                req.DocumentFile.FileName,
                req.DocumentFile.OpenReadStream(),
                req.DocumentFile.ContentType)
            : null;

        var result = await mediator.Send(new UpdateAnimalHealthCommand(
            req.AnimalId,
            req.HealthRecordId,
            req.OccurredOn,
            req.Description,
            documentFile,
            req.DeleteDocument ?? false
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}