using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class UpdateAnimal(IMediator mediator) : Endpoint<UpdateAnimalRequest, UpdateAnimalCommandResponse>
{
    public override void Configure()
    {
        Put(UpdateAnimalRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        AllowFormData();
        AllowFileUploads();
    }

    public override async Task HandleAsync(UpdateAnimalRequest req, CancellationToken ct)
    {
        var signatureResult = AnimalSignature.Create(req.Signature);
        if (signatureResult.IsFailure)
        {
            await this.SendResultAsync(
                Result<UpdateAnimalCommandResponse>.ValidationError(signatureResult.Error!), ct);
            return;
        }

        var photos = new List<PhotoUploadInfo>();
        foreach (var file in req.NewPhotos)
        {
            var stream = file.OpenReadStream();
            photos.Add(new PhotoUploadInfo(file.FileName, stream, file.ContentType));
        }

        var result = await mediator.Send(new UpdateAnimalCommand(
            req.Id,
            signatureResult.Value!,
            req.TransponderCode,
            req.Name,
            req.Color,
            req.Species,
            req.Sex,
            req.BirthDate,
            photos,
            req.ExistingPhotoIds,
            req.MainPhotoId,
            req.MainPhotoIndex
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}