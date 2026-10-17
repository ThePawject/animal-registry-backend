using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class DeleteVaccinationAttachmentCommandHandler(
    IAnimalVaccinationRepository vaccinationRepository,
    IAnimalsUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteVaccinationAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteVaccinationAttachmentCommand request, CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationRepository.GetByIdAsync(request.VaccinationId, request.AnimalId,
            currentUser.ShelterId, cancellationToken);
        if (vaccination is null)
        {
            return Result.NotFound("Vaccination not found.");
        }

        if (vaccination.Attachment is not { } attachment)
        {
            return Result.Success();
        }

        vaccination.RemoveAttachment(DateTimeOffset.UtcNow);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult;
        }

        await blobStorageService.DeleteAsync(attachment.BlobPath, cancellationToken);
        return Result.Success();
    }
}
