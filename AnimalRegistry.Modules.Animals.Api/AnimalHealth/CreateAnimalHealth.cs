using AnimalRegistry.Modules.Animals.Application.AnimalHealth;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class CreateAnimalHealth(IMediator mediator) : Endpoint<CreateAnimalHealthRequest>
{
    public override void Configure()
    {
        Post(CreateAnimalHealthRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        AllowFormData();
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Adds a new health record to an animal.";
            s.Description = "Adds a new health record to an animal with optional document upload.";
        });
    }

    public override async Task HandleAsync(CreateAnimalHealthRequest req, CancellationToken ct)
    {
        var documentFile = req.DocumentFile != null
            ? new DocumentUploadInfo(
                req.DocumentFile.FileName,
                req.DocumentFile.OpenReadStream(),
                req.DocumentFile.ContentType)
            : null;

        var result = await mediator.Send(new CreateAnimalHealthCommand(
            req.AnimalId,
            req.OccurredOn,
            req.Description,
            documentFile
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}