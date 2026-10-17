using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.Vaccinations;

internal sealed class UploadVaccinationAttachmentValidator : Validator<UploadVaccinationAttachmentRequest>
{
    public UploadVaccinationAttachmentValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.VaccinationId).NotEmpty();
        RuleFor(x => x.File).NotNull();

        When(x => x.File != null, () =>
        {
            RuleFor(x => x.File)
                .Must(file => file!.Length <= 10 * 1024 * 1024)
                .WithMessage("Attachment must be 10MB or less");

            RuleFor(x => x.File)
                .Must(file => IsAllowedDocumentType(file!.ContentType))
                .WithMessage("Unsupported attachment type. Allowed types: PDF, DOC, DOCX, JPG, JPEG, PNG, WEBP");
        });
    }

    private static bool IsAllowedDocumentType(string contentType)
    {
        var allowedTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        return allowedTypes.Contains(contentType.ToLowerInvariant());
    }
}
