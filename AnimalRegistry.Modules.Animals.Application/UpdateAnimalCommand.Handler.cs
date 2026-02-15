using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class UpdateAnimalCommandHandler(
    IAnimalRepository animalRepository,
    IAnimalSignatureService signatureService,
    ICurrentUser currentUser,
    IBlobStorageService blobStorageService)
    : IRequestHandler<UpdateAnimalCommand, Result<UpdateAnimalCommandResponse>>
{
    public async Task<Result<UpdateAnimalCommandResponse>> Handle(
        UpdateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(
            request.Id,
            currentUser.ShelterId,
            cancellationToken);

        if (animal == null)
        {
            return Result<UpdateAnimalCommandResponse>.NotFound(
                $"Animal with id {request.Id} not found.");
        }

        if (request.Signature.Value != animal.Signature.Value)
        {
            var isUnique = await signatureService.IsSignatureUniqueAsync(
                request.Signature.Value,
                currentUser.ShelterId,
                request.Id,
                cancellationToken);

            if (!isUnique)
            {
                return Result<UpdateAnimalCommandResponse>.ValidationError(
                    $"Signature '{request.Signature.Value}' is already in use.");
            }
        }

        animal.Update(
            request.Signature,
            request.TransponderCode,
            request.Name,
            request.Color,
            request.Species,
            request.Sex,
            request.BirthDate);

        var currentPhotoIds = animal.Photos.Select(p => p.Id).ToHashSet();
        var requestedPhotoIds = request.ExistingPhotoIds.ToHashSet();

        var photosToRemove = currentPhotoIds.Except(requestedPhotoIds).ToList();
        var deleteTasks = photosToRemove.Select(async photoId =>
        {
            var photo = animal.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo != null)
            {
                await blobStorageService.DeleteAsync(photo.BlobPath, cancellationToken);
                animal.RemovePhoto(photoId);
            }
        });
        await Task.WhenAll(deleteTasks);

        if (request.NewPhotos.Count > 0)
        {
            var uploadTasks = request.NewPhotos.Select((photo, index) =>
                UploadPhotoAsync(photo, index, animal.Id, cancellationToken));

            var results = await Task.WhenAll(uploadTasks);

            foreach (var (uploadResult, index) in results.Select((r, i) => (r, i)))
            {
                if (uploadResult.IsFailure)
                {
                    return Result<UpdateAnimalCommandResponse>.ValidationError(uploadResult.Error!);
                }

                var isMain = request.MainPhotoIndex.HasValue && request.MainPhotoIndex.Value == index;
                animal.AddPhoto(uploadResult.Value!, request.NewPhotos[index].FileName, isMain);
            }
        }

        if (request.MainPhotoId.HasValue)
        {
            animal.SetMainPhoto(request.MainPhotoId.Value);
        }

        var result = await animalRepository.UpdateAsync(animal, cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            return Result<UpdateAnimalCommandResponse>.Failure("Failed to update animal.");
        }

        return Result<UpdateAnimalCommandResponse>.Success(
            new UpdateAnimalCommandResponse(animal.Id));
    }

    private async Task<Result<string>> UploadPhotoAsync(PhotoUploadInfo photo, int index, Guid animalId, CancellationToken cancellationToken)
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