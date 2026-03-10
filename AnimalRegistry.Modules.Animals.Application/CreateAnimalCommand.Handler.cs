using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class CreateAnimalCommandHandler(
    IAnimalRepository animalRepository,
    IAnimalSignatureService signatureService,
    ICurrentUser currentUser,
    IBlobStorageService blobStorageService)
    : IRequestHandler<CreateAnimalCommand, Result<CreateAnimalCommandResponse>>
{
    public async Task<Result<CreateAnimalCommandResponse>> Handle(CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var isUnique = await signatureService.IsSignatureUniqueAsync(
            request.Signature.Value,
            currentUser.ShelterId,
            null,
            cancellationToken);

        if (!isUnique)
        {
            return Result<CreateAnimalCommandResponse>.ValidationError(
                $"Signature '{request.Signature.Value}' is already in use.");
        }

        var animal = Animal.Create(
            request.Signature,
            request.TransponderCode,
            request.Name,
            request.Color,
            request.Species,
            request.Sex,
            request.BirthDate,
            currentUser.ShelterId
        );

        if (request.Photos.Count > 0)
        {
            var uploadTasks = request.Photos.Select((photo, index) =>
                UploadPhotoAsync(photo, index, animal.Id, cancellationToken));

            var results = await Task.WhenAll(uploadTasks);

            foreach (var (uploadResult, index) in results.Select((r, i) => (r, i)))
            {
                if (uploadResult.IsFailure)
                {
                    return Result<CreateAnimalCommandResponse>.ValidationError(uploadResult.Error!);
                }

                var isMain = request.MainPhotoIndex.HasValue && request.MainPhotoIndex.Value == index;
                animal.AddPhoto(uploadResult.Value!, request.Photos[index].FileName, isMain);
            }
        }

        var result = await animalRepository.AddAsync(animal, cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            return Result<CreateAnimalCommandResponse>.Failure("Failed to create animal.");
        }

        return Result<CreateAnimalCommandResponse>.Success(new CreateAnimalCommandResponse(result.Value.Id));
    }

    private async Task<Result<string>> UploadPhotoAsync(PhotoUploadInfo photo, int index, Guid animalId,
        CancellationToken cancellationToken)
    {
        return await blobStorageService.UploadAsync(
            photo.FileName,
            photo.Content,
            photo.ContentType,
            currentUser.ShelterId,
            animalId,
            cancellationToken);
    }
}