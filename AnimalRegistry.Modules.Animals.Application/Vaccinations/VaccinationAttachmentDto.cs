namespace AnimalRegistry.Modules.Animals.Application.Vaccinations;

public sealed record VaccinationAttachmentDto(
    string FileName,
    string ContentType,
    DateTimeOffset UploadedOn,
    string Url);
