using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;
using System.Text.RegularExpressions;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimalValidator : Validator<CreateAnimalRequest>
{
    private static readonly Regex SignaturePattern = new(@"^(\d{4})/(\d{4})$", RegexOptions.Compiled);

    public CreateAnimalValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty()
            .Must(BeValidSignatureFormat)
            .WithMessage("Invalid signature format. Expected format: YYYY/NNNN (e.g., 2026/0001).");

        RuleFor(x => x.TransponderCode)
            .MaximumLength(100);
        RuleFor(x => x.Name)
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(x => x.Color)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Species)
            .NotEmpty()
            .IsInEnum();
        RuleFor(x => x.Sex)
            .NotEmpty()
            .IsInEnum();
        RuleFor(x => x.MainPhotoIndex)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MainPhotoIndex.HasValue);

        RuleFor(x => x.Photos)
            .Must(photos => photos.Count <= 10)
            .WithMessage("Maximum 10 photos allowed");

        RuleForEach(x => x.Photos)
            .Must(file => file.Length <= 10 * 1024 * 1024)
            .WithMessage("Each photo must be 10MB or less");

        RuleFor(x => x.Photos)
            .Must(photos => photos.Sum(f => f.Length) <= 100 * 1024 * 1024)
            .WithMessage("Total photo size must not exceed 100MB");
    }

    private static bool BeValidSignatureFormat(string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        return SignaturePattern.IsMatch(signature.Trim()) && AnimalSignature.Create(signature).IsSuccess;
    }
}