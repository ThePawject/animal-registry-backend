using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api.AnimalHealth;

public sealed class CreateAnimalHealthValidator : Validator<CreateAnimalHealthRequest>
{
    public CreateAnimalHealthValidator()
    {
        RuleFor(x => x.AnimalId).NotEmpty();
        RuleFor(x => x.OccurredOn).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);

        When(x => x.DocumentFile != null, () =>
        {
            RuleFor(x => x.DocumentFile)
                .Must(file => file!.Length <= 10 * 1024 * 1024)
                .WithMessage("Document must be 10MB or less");

            RuleFor(x => x.DocumentFile)
                .Must(file => IsAllowedDocumentType(file!.ContentType))
                .WithMessage("Unsupported document type. Allowed types: PDF, DOCX, JPG, JPEG, PNG, WEBP");
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