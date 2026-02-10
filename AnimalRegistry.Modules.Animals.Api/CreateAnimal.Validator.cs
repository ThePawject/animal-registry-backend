using AnimalRegistry.Modules.Animals.Domain.Animals;
using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class CreateAnimalValidator : Validator<CreateAnimalRequest>
{
    public CreateAnimalValidator()
    {
        RuleFor(x => x.Signature)
            .NotEmpty();
        RuleFor(x => x.TransponderCode)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2);
        RuleFor(x => x.Color)
            .NotEmpty();
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

        RuleFor(x => x.MainPhotoIndex)
            .Must((request, index) => !index.HasValue || (index.Value < request.Photos.Count && index.Value >= 0))
            .WithMessage("MainPhotoIndex must point to one of the uploaded photos");
    }
}