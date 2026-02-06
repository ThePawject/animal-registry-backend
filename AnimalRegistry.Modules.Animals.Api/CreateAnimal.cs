using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimal(IMediator mediator) : Endpoint<CreateAnimalRequest, CreateAnimalCommandResponse>
{
    public override void Configure()
    {
        Post(CreateAnimalRequest.Route);
        Policies(ShelterAccessHandler.ShelterAccessPolicyName);
        AllowFormData();
        AllowFileUploads();
    }

    public override async Task HandleAsync(CreateAnimalRequest req, CancellationToken ct)
    {
        var photos = new List<PhotoUploadInfo>();
        foreach (var file in req.Photos)
        {
            var stream = file.OpenReadStream();
            photos.Add(new PhotoUploadInfo(file.FileName, stream, file.ContentType));
        }

        var result = await mediator.Send(new CreateAnimalCommand(
            req.Signature,
            req.TransponderCode,
            req.Name,
            req.Color,
            req.Species,
            req.Sex,
            req.BirthDate,
            photos,
            req.MainPhotoIndex
        ), ct);

        await this.SendResultAsync(result, ct);
    }
}