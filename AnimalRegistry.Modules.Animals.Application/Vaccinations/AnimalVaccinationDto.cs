using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;

namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public sealed record AnimalVaccinationDto(
    Guid Id,
    Guid AnimalId,
    string VaccineName,
    DateTimeOffset AdministeredOn,
    DateTimeOffset? ValidUntil,
    string? Veterinarian,
    string? BatchNumber,
    string? Notes,
    VaccinationStatus Status,
    int? DaysUntilExpiry,
    Guid? ReplacesVaccinationId,
    Guid? ReplacedByVaccinationId,
    VaccinationAttachmentDto? Attachment,
    string CreatedBy,
    DateTimeOffset CreatedOn,
    DateTimeOffset ModifiedOn)
{
    public static AnimalVaccinationDto FromDomain(AnimalVaccination vaccination, Guid? replacedByVaccinationId,
        DateTimeOffset now, IBlobStorageService blobStorageService)
    {
        return new AnimalVaccinationDto(
            vaccination.Id,
            vaccination.AnimalId,
            vaccination.VaccineName,
            vaccination.AdministeredOn,
            vaccination.ValidUntil,
            vaccination.Veterinarian,
            vaccination.BatchNumber,
            vaccination.Notes,
            vaccination.GetStatus(now, replacedByVaccinationId is not null),
            vaccination.DaysUntilExpiry(now),
            vaccination.ReplacesVaccinationId,
            replacedByVaccinationId,
            vaccination.Attachment is { } attachment
                ? new VaccinationAttachmentDto(
                    attachment.FileName,
                    attachment.ContentType,
                    attachment.UploadedOn,
                    blobStorageService.GetBlobUrl(attachment.BlobPath))
                : null,
            vaccination.CreatedBy,
            vaccination.CreatedOn,
            vaccination.ModifiedOn);
    }
}
