using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

internal sealed class CreateAnimalHealthCommandHandler(
    IAnimalRepository animalRepository,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser
) : IRequestHandler<CreateAnimalHealthCommand, Result>
{
    public async Task<Result> Handle(CreateAnimalHealthCommand request, CancellationToken cancellationToken)
    {
        var animal = await animalRepository.GetByIdAsync(request.AnimalId, currentUser.ShelterId, cancellationToken);
        if (animal is null)
        {
            return Result.NotFound("Animal not found.");
        }

        animal.AddHealthRecord(request.OccurredOn, request.Description, currentUser.Email);

        if (request.DocumentFile != null)
        {
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

            var healthRecord = animal.HealthRecords.Last();
            var document = AnimalHealthDocument.Create(healthRecord.Id, uploadResult.Value!, request.DocumentFile.FileName, request.DocumentFile.ContentType);
            healthRecord.SetDocument(document);
        }

        await animalRepository.UpdateAsync(animal, cancellationToken);

        return Result.Success();
    }
}