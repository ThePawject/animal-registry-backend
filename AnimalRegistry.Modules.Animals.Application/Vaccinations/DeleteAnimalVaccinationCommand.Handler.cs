using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class DeleteAnimalVaccinationCommandHandler(
    IAnimalVaccinationRepository vaccinationRepository,
    IAnimalsUnitOfWork unitOfWork,
    AttachmentBlobCleaner blobCleaner,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteAnimalVaccinationCommand, Result>
{
    public async Task<Result> Handle(DeleteAnimalVaccinationCommand request, CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationRepository.GetByIdAsync(request.VaccinationId, request.AnimalId,
            currentUser.ShelterId, cancellationToken);
        if (vaccination is null)
        {
            return Result.NotFound("Vaccination not found.");
        }

        var replacing = await vaccinationRepository.FindReplacingAsync(vaccination.Id, currentUser.ShelterId,
            cancellationToken);
        replacing?.ClearReplacement(DateTimeOffset.UtcNow);

        var attachment = vaccination.Attachment;
        vaccinationRepository.Remove(vaccination);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult;
        }

        if (attachment is not null)
        {
            await blobCleaner.DeleteAsync(attachment.BlobPath);
        }

        return Result.Success();
    }
}
