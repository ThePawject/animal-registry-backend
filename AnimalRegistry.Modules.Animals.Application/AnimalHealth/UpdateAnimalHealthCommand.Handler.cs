using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class UpdateAnimalHealthCommandHandler(
    IAnimalRepository animalRepository,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser
) : IRequestHandler<UpdateAnimalHealthCommand, Result>
{
    public async Task<Result> Handle(UpdateAnimalHealthCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        var healthRecord = animal.HealthRecords.FirstOrDefault(h => h.Id == request.HealthRecordId);
        if (healthRecord is null)
        {
            return Result.NotFound("Health record not found.");
        }

        if (request.DeleteDocument && healthRecord.Document != null)
        {
            var oldDocument = healthRecord.Document;
            await blobStorageService.DeleteAsync(oldDocument.BlobPath, cancellationToken);
            healthRecord.RemoveDocument();
        }
        else if (request.DocumentFile != null)
        {
            if (healthRecord.Document != null)
            {
                var oldDocument = healthRecord.Document;
                await blobStorageService.DeleteAsync(oldDocument.BlobPath, cancellationToken);
            }

            var uploadResult = await blobStorageService.UploadDocumentAsync(
                request.DocumentFile.FileName,
                request.DocumentFile.Content,
                request.DocumentFile.ContentType,
                currentUser.ShelterId,
                request.AnimalId,
                cancellationToken
            );

            if (uploadResult.IsFailure)
            {
                return Result.ValidationError(uploadResult.Error!);
            }

            var document = AnimalHealthDocument.Create(healthRecord.Id, uploadResult.Value!, request.DocumentFile.FileName, request.DocumentFile.ContentType);
            healthRecord.SetDocument(document);
        }

        animal.UpdateHealthRecord(request.HealthRecordId, request.OccurredOn, request.Description);

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}
