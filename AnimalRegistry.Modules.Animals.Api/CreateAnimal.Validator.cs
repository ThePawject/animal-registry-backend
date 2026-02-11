using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimalValidator : Validator<CreateAnimalRequest>
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.TransponderCode)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Name)
            .NotEmpty()
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
        RuleFor(x => x.BirthDate)
            .NotEmpty();
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
}