using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.CurrentUser;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application;

internal sealed class UpdateAnimalCommandHandler(
    IAnimalRepository animalRepository,
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
        foreach (var photoId in photosToRemove)
        {
            var photo = animal.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo != null)
            {
                await blobStorageService.DeleteAsync(photo.BlobPath, cancellationToken);
                animal.RemovePhoto(photoId);
            }
        }

        for (var i = 0; i < request.NewPhotos.Count; i++)
        {
            var photo = request.NewPhotos[i];

            var uploadResult = await blobStorageService.UploadAsync(
                photo.FileName,
                photo.Content,
                photo.ContentType,
                currentUser.ShelterId,
                animal.Id,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                return Result<UpdateAnimalCommandResponse>.ValidationError(
                    $"Error uploading photo '{photo.FileName}': {uploadResult.Error}");
            }

            var isMain = request.MainPhotoIndex.HasValue && request.MainPhotoIndex.Value == i;
            animal.AddPhoto(uploadResult.Value!, photo.FileName, isMain);
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
}
