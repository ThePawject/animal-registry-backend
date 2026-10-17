using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.Access;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

internal sealed class SetVaccinationAttachmentCommandHandler(
    IAnimalVaccinationRepository vaccinationRepository,
    IAnimalsUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    ICurrentUser currentUser)
    : IRequestHandler<SetVaccinationAttachmentCommand, Result<AnimalVaccinationDto>>
{
    public async Task<Result<AnimalVaccinationDto>> Handle(SetVaccinationAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var vaccination = await vaccinationRepository.GetByIdAsync(request.VaccinationId, request.AnimalId,
            currentUser.ShelterId, cancellationToken);
        if (vaccination is null)
        {
            return Result<AnimalVaccinationDto>.NotFound("Vaccination not found.");
        }

        var uploadResult = await blobStorageService.UploadDocumentAsync(
            request.File.FileName,
            request.File.Content,
            request.File.ContentType,
            vaccination.ShelterId,
            vaccination.AnimalId,
            cancellationToken);
        if (uploadResult.IsFailure)
        {
            return Result<AnimalVaccinationDto>.ValidationError(uploadResult.Error!);
        }

        var now = DateTimeOffset.UtcNow;
        var previous = vaccination.Attachment;
        vaccination.SetAttachment(
            VaccinationAttachment.Create(uploadResult.Value!, request.File.FileName, request.File.ContentType, now),
            now);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            await blobStorageService.DeleteAsync(uploadResult.Value!, cancellationToken);
            return Result<AnimalVaccinationDto>.ValidationError(saveResult.Error!);
        }

        if (previous is not null)
        {
            await blobStorageService.DeleteAsync(previous.BlobPath, cancellationToken);
        }

        return Result<AnimalVaccinationDto>.Success(
            AnimalVaccinationDto.FromDomain(vaccination, now, blobStorageService));
    }
}
